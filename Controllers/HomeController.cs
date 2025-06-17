using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GoogleOAuthDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // 🔓 Public landing page
        public IActionResult Index()
        {
            return View();
        }

        // 🔓 Public privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ Logged-in Dashboard with role display
        [Authorize]
        public IActionResult Dashboard()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? User.Identity?.Name ?? "User";

            // 🔍 Extract user role from claims
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            string role = roleClaim?.Value ?? "User";

            ViewBag.Role = role;
            return View();
        }

        // ✅ Upload page: Only Admins can access
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // ✅ Handle file uploads securely
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Upload(string subject, IFormFile file)
        {
            if (string.IsNullOrEmpty(subject) || file == null || file.Length == 0)
            {
                ViewBag.Message = "⚠️ Please provide a valid subject and file.";
                return View();
            }

            // Create subject-based directory
            var uploadDir = Path.Combine("UploadedMaterials", subject);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, uploadDir);
            Directory.CreateDirectory(fullPath);

            var fileName = Path.GetFileName(file.FileName);
            var savedFilePath = Path.Combine(fullPath, fileName);

            // Save uploaded file to server
            using (var stream = new FileStream(savedFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Get logged-in user's email
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown@example.com";

            var material = new UploadedMaterial
            {
                Subject = subject,
                FilePath = Path.Combine(uploadDir, fileName).Replace("\\", "/"),
                FileName = fileName,
                UserEmail = userEmail,
                UploadedAt = DateTime.UtcNow
            };

            _context.UploadedMaterials.Add(material);
            await _context.SaveChangesAsync();

            ViewBag.Message = "✅ Upload successful!";
            return View();
        }

        // 🔐 Show error trace when exceptions happen
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
