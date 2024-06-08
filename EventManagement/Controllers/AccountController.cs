using EventManagement.DbContext;
using EventManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventManagement.Helper;
using System.Text;

namespace EventManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.MobileNumber == model.Mobile);
                var p = PasswordHelper.HashPassword(model.Password);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.UserEmail),
                        new("FullName", user.FirstName+" "+user.LastName),
                        new(ClaimTypes.Role, user.Role),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        // Allow refresh
                        AllowRefresh = true,
                        // Set expiration time
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties).Wait();

                    HttpContext.LogInUser();
                    UserHelper.User = new User();
                    UserHelper.User = user;
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            HttpContext.LogOutUser();
            return RedirectToAction("Index", "Account");
        }
    }
}
