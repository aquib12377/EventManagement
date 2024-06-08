using EventManagement.DbContext;
using EventManagement.Helper;
using EventManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace EventManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private IActionResult EnsureLoggedIn(Func<IActionResult> action)
        {
            if (HttpContext.IsLoggedIn())
                return action();

            return RedirectToAction("Index", "Account");
        }

        public IActionResult Index() => EnsureLoggedIn(() => View(nameof(Index)));
        public IActionResult EventAttendance() => EnsureLoggedIn(() => View(nameof(EventAttendance)));
        public IActionResult EventMealAttendance() => EnsureLoggedIn(() => View(nameof(EventMealAttendance)));
        public IActionResult ViewEvent() => EnsureLoggedIn(() => View(nameof(ViewEvent)));
        public IActionResult Events() => EnsureLoggedIn(() => View(nameof(Events)));
        public IActionResult AddEvents() => EnsureLoggedIn(() => View(nameof(AddEvents)));
        public IActionResult Privacy() => View();

        public IActionResult AddUser(int u)
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