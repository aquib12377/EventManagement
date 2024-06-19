using EventManagement.DbContext;
using EventManagement.Helper;
using EventManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using SessionExtensions = EventManagement.Helper.SessionExtensions;

namespace EventManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext,User user,UserManager<User> userManager)
        {
            _logger = logger;
            _dbContext = dbContext;
            this.userManager = userManager;
            //UserHelper.User = user;
        }

        private IActionResult EnsureLoggedIn(Func<IActionResult> action)
        {
            var userId = HttpContext.Session.GetString("UID");

            if (!string.IsNullOrEmpty(userId))
            {
                return action();
            }
            return RedirectToAction("Index", "Account");
        }


        public IActionResult Index() => EnsureLoggedIn(() => View(nameof(Index)));
        public IActionResult EventAttendance() => EnsureLoggedIn(() => View(nameof(EventAttendance)));
        public IActionResult EventMealAttendance() => EnsureLoggedIn(() => View(nameof(EventMealAttendance)));
        public IActionResult ViewEvent() => EnsureLoggedIn(() => View(nameof(ViewEvent)));
        public IActionResult Events() => EnsureLoggedIn(() => View(nameof(Events)));
        public IActionResult AddEvents() => EnsureLoggedIn(() => View(nameof(AddEvents)));
        public IActionResult Privacy() => View();

        public IActionResult AddUser(string u)
        {
            var result = _dbContext.Users.AsNoTracking().FirstOrDefault(x => x.UserId == u);

            if (result == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("AddUser");
            }

            return View("AddUser", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}