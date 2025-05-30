using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    public IActionResult Login()
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
            return RedirectToAction("Login");
        }

        var userName = result.Principal.Identity?.Name;
        HttpContext.Session.SetString("UserName", userName ?? "Guest");

        // 🔄 Redirect to AI Dashboard after successful login
        return RedirectToAction("Dashboard", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
