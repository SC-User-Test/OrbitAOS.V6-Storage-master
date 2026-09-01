using Microsoft.AspNetCore.Mvc;
using OrbitAOS.Web.Models;
using System.Diagnostics;

namespace OrbitAOS.Web.Controllers
{
    /// <summary>
    /// Home controller providing the main landing pages.
    /// Migrated from ASP.NET MVC 5 Controller to ASP.NET Core MVC Controller on .NET 8.
    /// Breaking changes addressed:
    ///   - System.Web.Mvc.Controller → Microsoft.AspNetCore.Mvc.Controller
    ///   - ActionResult → IActionResult (more flexible return type)
    ///   - ILogger injected via constructor DI (replaces static logging)
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        /// <param name="logger">The logger instance injected by the DI container.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the application home page.
        /// </summary>
        /// <returns>The Index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>The Privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page.
        /// ResponseCache attribute replaces legacy OutputCache attribute.
        /// </summary>
        /// <returns>The Error view with request ID.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
