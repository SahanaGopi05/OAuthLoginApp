using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GoogleOAuthDemo.Controllers
{
    [Authorize]
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
        public IActionResult Index(string? subject = null)
        {
            string uploadsRoot = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedMaterials");

            var subjects = Directory.Exists(uploadsRoot)
                ? Directory.GetDirectories(uploadsRoot)
                    .Select(Path.GetFileName)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList()
                : new List<string>();

            ViewBag.Subjects = subjects;
            ViewBag.SelectedSubject = subject;
            ViewBag.Submitted = false;

            List<(string, string)> questions = new();

            if (!string.IsNullOrEmpty(subject))
            {
                string subjectPath = Path.Combine(uploadsRoot, subject);
                if (Directory.Exists(subjectPath))
                {
                    var files = Directory.GetFiles(subjectPath);

                    foreach (var file in files)
                    {
                        var lines = System.IO.File.ReadAllLines(file);
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line) && line.Contains("::"))
                            {
                                var parts = line.Split(new string[] { "::" }, StringSplitOptions.None);
                                if (parts.Length >= 2)
                                {
                                    string questionPart = Regex.Replace(parts[0].Trim(), @"^Q\d*\s*:\s*", "", RegexOptions.IgnoreCase);
                                    string answerPart = parts[1].Trim();

                                    if (!string.IsNullOrEmpty(questionPart) && !string.IsNullOrEmpty(answerPart))
                                    {
                                        questions.Add((questionPart, answerPart));
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
        public async Task<IActionResult> Index(string subject, List<string> userAnswers, List<string> correctAnswers, List<string> originalQuestions)
        {
            ViewBag.Subjects = new List<string>();
            ViewBag.SelectedSubject = subject;
            ViewBag.Submitted = true;

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
                Username = User.Identity?.Name ?? "Anonymous",
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
