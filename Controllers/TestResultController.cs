using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoogleOAuthDemo.Controllers
{
    public class TestResultController : Controller
    {
        private readonly AppDbContext _context;

        public TestResultController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var results = await _context.TestResults.ToListAsync();
            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult(TestResult result)
        {
            if (ModelState.IsValid)
            {
                result.DateTaken = DateTime.Now;
                _context.TestResults.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return BadRequest("Invalid data");
        }
    }
}
