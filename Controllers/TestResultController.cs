using GoogleOAuthDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace GoogleOAuthDemo.Controllers
{
    [Authorize] // ✅ Ensure the user is logged in
    public class TestResultController : Controller
    {
        private readonly AppDbContext _context;

        public TestResultController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ✅ Show only the current user's results
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var userResults = await _context.TestResults
                .Where(r => r.UserEmail == userEmail)
                .OrderByDescending(r => r.DateTaken)
                .ToListAsync();

            return View(userResults);
        }

        // ✅ Called by QuizController to save the result (all users)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(TestResult result)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var username = User.Identity?.Name ?? "Anonymous";

            if (ModelState.IsValid)
            {
                result.UserEmail = userEmail ?? "anonymous@example.com";
                result.Username = username;
                result.DateTaken = DateTime.Now;

                _context.TestResults.Add(result);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Quiz");
            }

            return BadRequest("Invalid test result data.");
        }
    }
}
