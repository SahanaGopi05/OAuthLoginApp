using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GoogleOAuthDemo.Controllers
{
    [Authorize] // ✅ Allow all authenticated users (Admin + Student)
    public class QuizController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public QuizController(IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? subject = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var subjects = await _context.UploadedMaterials
                .Select(u => u.Subject)
                .Distinct()
                .ToListAsync();

            ViewBag.Subjects = subjects;
            ViewBag.SelectedSubject = subject;
            ViewBag.Submitted = false;

            List<(string, string)> questions = new();

            if (!string.IsNullOrEmpty(subject))
            {
                var materials = await _context.UploadedMaterials
                    .Where(u => u.Subject == subject)
                    .ToListAsync();

                foreach (var material in materials)
                {
                    var fullPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        material.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(fullPath))
                    {
                        var lines = System.IO.File.ReadAllLines(fullPath);
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line) && line.Contains("::"))
                            {
                                var parts = line.Split("::", StringSplitOptions.None);
                                if (parts.Length >= 2)
                                {
                                    string question = Regex.Replace(parts[0].Trim(), @"^Q\d*\s*:\s*", "", RegexOptions.IgnoreCase);
                                    string answer = parts[1].Trim();

                                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                                    {
                                        questions.Add((question, answer));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var selectedQuestions = questions.OrderBy(q => Guid.NewGuid()).Take(5).ToList();
            ViewBag.Questions = selectedQuestions;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string subject, List<string>? userAnswers, List<string>? correctAnswers, List<string>? originalQuestions)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var username = User.Identity?.Name ?? "Anonymous";

            ViewBag.SelectedSubject = subject;
            ViewBag.Submitted = true;

            ViewBag.Subjects = await _context.UploadedMaterials
                .Select(u => u.Subject)
                .Distinct()
                .ToListAsync();

            if (userAnswers == null || correctAnswers == null || originalQuestions == null ||
                userAnswers.Count != correctAnswers.Count || userAnswers.Count != originalQuestions.Count)
            {
                ViewBag.ErrorMessage = "⚠️ Submission error: Please answer all questions before submitting.";
                ViewBag.Questions = new List<(string, string)>();
                ViewBag.Score = 0;
                return View();
            }

            int score = 0;
            List<(string Question, string UserAnswer, string CorrectAnswer)> results = new();

            string Normalize(string input) =>
                Regex.Replace(input ?? "", @"[\p{P}]", "").Trim().ToLowerInvariant();

            for (int i = 0; i < correctAnswers.Count; i++)
            {
                string normalizedUser = Normalize(userAnswers[i]);
                string normalizedCorrect = Normalize(correctAnswers[i]);

                if (normalizedUser == normalizedCorrect)
                    score++;

                results.Add((originalQuestions[i], userAnswers[i], correctAnswers[i]));
            }

            ViewBag.Questions = results;
            ViewBag.Score = score;

            var result = new TestResult
            {
                Username = username,
                UserEmail = userEmail ?? "anonymous@example.com",
                Subject = subject,
                Score = score,
                DateTaken = DateTime.Now
            };

            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();

            return View();
        }
    }
}
