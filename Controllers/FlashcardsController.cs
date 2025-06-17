using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoogleOAuthDemo.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoogleOAuthDemo.Controllers
{
    [Authorize] // ✅ All logged-in users can access
    public class FlashcardsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public FlashcardsController(
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string subject = null)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(); // Just a safety check

            var flashcards = new List<(string, string)>();

            // ✅ Get all available subjects from the database
            var subjects = await _context.UploadedMaterials
                .Select(m => m.Subject)
                .Distinct()
                .ToListAsync();

            if (!string.IsNullOrEmpty(subject))
            {
                var materials = await _context.UploadedMaterials
                    .Where(m => m.Subject == subject)
                    .ToListAsync();

                foreach (var material in materials)
                {
                    string safePath = material.FilePath?.TrimStart('/');
                    if (!string.IsNullOrEmpty(safePath))
                    {
                        string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, safePath.Replace('/', Path.DirectorySeparatorChar));

                        if (System.IO.File.Exists(fullPath))
                        {
                            var lines = System.IO.File.ReadAllLines(fullPath);
                            foreach (var line in lines)
                            {
                                var parts = line.Split("::");
                                if (parts.Length == 2)
                                {
                                    flashcards.Add((parts[0].Trim(), parts[1].Trim()));
                                }
                            }
                        }
                    }
                }
            }

            ViewBag.Subjects = subjects;
            ViewBag.SelectedSubject = subject;
            ViewBag.Flashcards = flashcards;

            return View();
        }
    }
}
