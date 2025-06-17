using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GoogleOAuthDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleOAuthDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Define your admin emails here
        private readonly List<string> _adminEmails = new List<string>
        {
            "admin1@example.com",
            "admin2@example.com"
            // Add more admin emails here
        };

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult ExternalLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.Identity?.Name ?? "Guest";

            // Check DB for existing user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Determine role based on email
            string assignedRole = _adminEmails.Contains(email) ? "Admin" : "Student";

            if (user == null)
            {
                // New user → create record with determined role
                user = new User
                {
                    Email = email,
                    Role = assignedRole
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Store in session (optional)
            HttpContext.Session.SetString("UserName", name);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Dashboard", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
