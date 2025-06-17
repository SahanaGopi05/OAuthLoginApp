using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Logging;
using GoogleOAuthDemo.Models;
using System;
using System.Collections.Generic;

namespace GoogleOAuthDemo.Controllers
{
    [Authorize] // ✅ All authenticated users can access
    public class UploadController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UploadController> _logger;

        public UploadController(AppDbContext context, ILogger<UploadController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // ✅ Removed Admin-only check
            string userEmail = User.FindFirstValue(ClaimTypes.Email);

            var userUploads = _context.UploadedMaterials
                .Where(m => m.UserEmail == userEmail)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            return View(userUploads);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string subject, IFormFile file)
        {
            // ✅ Removed Admin-only check
            string userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (file != null && file.Length > 0 && !string.IsNullOrWhiteSpace(subject))
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "UploadedMaterials", subject);
                Directory.CreateDirectory(uploadPath);

                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/UploadedMaterials/{subject}/{fileName}";

                var upload = new UploadedMaterial
                {
                    UserEmail = userEmail,
                    Subject = subject,
                    FileName = fileName,
                    FilePath = fileUrl,
                    UploadedAt = DateTime.UtcNow
                };

                _context.UploadedMaterials.Add(upload);
                await _context.SaveChangesAsync();

                ViewBag.Message = $"✅ File uploaded successfully to <b>{subject}</b>!<br><a class='text-info' href='{fileUrl}' target='_blank'>📂 View File</a>";
            }
            else
            {
                ViewBag.Message = "⚠️ Please provide a valid subject and file.";
            }

            var userUploads = _context.UploadedMaterials
                .Where(m => m.UserEmail == userEmail)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            return View(userUploads);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // ✅ Removed Admin-only check
            string userEmail = User.FindFirstValue(ClaimTypes.Email);
            var upload = _context.UploadedMaterials
                .FirstOrDefault(m => m.Id == id && m.UserEmail == userEmail);

            if (upload != null)
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedMaterials", upload.Subject, upload.FileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                _context.UploadedMaterials.Remove(upload);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ File deleted successfully.";
            }
            else
            {
                TempData["Message"] = "⚠️ File not found or unauthorized access.";
            }

            return RedirectToAction("Index");
        }
    }
}
