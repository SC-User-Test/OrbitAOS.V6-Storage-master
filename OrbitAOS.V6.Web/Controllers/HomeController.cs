using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Web.Models;
using System.Diagnostics;

namespace OrbitAOS.V6.Web.Controllers;

/// <summary>
/// Home controller providing the main landing page, privacy page, and error handling.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="HomeController"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>Displays the application home page.</summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Displays the privacy policy page.</summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>Displays the error page with request tracking information.</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
