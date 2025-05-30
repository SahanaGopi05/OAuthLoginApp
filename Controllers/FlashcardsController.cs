using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FlashcardsController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FlashcardsController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public IActionResult Index(string subject = null)
    {
        var flashcards = new List<(string, string)>();
        string uploadRoot = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedMaterials");

        // Step 1: Get all subjects from folders
        var subjects = Directory.Exists(uploadRoot)
            ? Directory.GetDirectories(uploadRoot).Select(Path.GetFileName).ToList()
            : new List<string>();

        // Step 2: If subject selected, parse the files
        if (!string.IsNullOrEmpty(subject))
        {
            string subjectPath = Path.Combine(uploadRoot, subject);
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
}
