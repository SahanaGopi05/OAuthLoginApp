using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GoogleOAuthDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "User";
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string subject, IFormFile file)
        {
            if (string.IsNullOrEmpty(subject) || file == null || file.Length == 0)
            {
                ViewBag.Message = "Please provide a valid subject and file.";
                return View();
            }

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subject);
            Directory.CreateDirectory(uploadPath); // Ensure directory exists

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ViewBag.Message = "Upload successful!";
            return View();
        }

        [HttpGet]
        public IActionResult Flashcards(string subject = null)
        {
            var flashcards = new List<(string, string)>();
            var uploadRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            // Get all subject folders
            var subjects = Directory.Exists(uploadRoot)
                ? Directory.GetDirectories(uploadRoot).Select(Path.GetFileName).ToList()
                : new List<string>();

            if (!string.IsNullOrEmpty(subject))
            {
                var subjectPath = Path.Combine(uploadRoot, subject);
                if (Directory.Exists(subjectPath))
                {
                    var files = Directory.GetFiles(subjectPath);
                    foreach (var file in files)
                    {
                        var lines = System.IO.File.ReadAllLines(file);
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

            ViewBag.Subjects = subjects;
            ViewBag.SelectedSubject = subject;
            ViewBag.Flashcards = flashcards;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
