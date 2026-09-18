using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Models;
using System.Diagnostics;

namespace OrbitAOS.V6.Controllers;

/// <summary>
/// Home controller providing the main landing page, privacy page, and error handling.
/// Migrated to ASP.NET Core MVC on .NET 8 with constructor-injected ILogger.
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

    /// <summary>Renders the application home/index page.</summary>
    public IActionResult Index()
    {
        _logger.LogInformation("Home page accessed.");
        return View();
    }

    /// <summary>Renders the privacy policy page.</summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Renders the error page. Response caching is disabled to ensure fresh error details.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
