using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class UploadController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string subject, IFormFile file)
    {
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
            ViewBag.Message = $"✅ File uploaded successfully to <b>{subject}</b>!<br><a class='text-info' href='{fileUrl}' target='_blank'>📂 View File</a>";
        }
        else
        {
            ViewBag.Message = "⚠️ Please provide a valid subject and file.";
        }

        return View();
    }
}
