using EventManagement.DbContext;
using EventManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventManagement.Helper;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private User _user;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public AccountController(ApplicationDbContext context,User user, SignInManager<User> signInManager,UserManager<User> userManager)
        {
            _context = context;
            _user = user;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var result = await signInManager.PasswordSignInAsync(model.Mobile, model.Password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(x => x.MobileNumber == model.Mobile && x.Password == model.Password);
                    if (user != null)
                    {
                        await signInManager.SignInAsync(user, isPersistent: true);

                        HttpContext.Session.SetString("UID", user.UserId);  // This line might be redundant now
                        HttpContext.Session.Set("UserData", user);  // This line might be redundant now
                        //UserHelper.User = user;
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Your account is locked out. Please try again later.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials. Please check your username and password.");
                }
            }

            // If ModelState is not valid, it will return to the login view with errors
            return View("Index", model);
            }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await signInManager.SignOutAsync();
            HttpContext.LogOutUser();
            return RedirectToAction("Index", "Account");
        }
    }
}
