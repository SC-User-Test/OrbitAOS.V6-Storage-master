using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;

namespace OrbitAOS.V6.Web.Controllers;

/// <summary>
/// Controller for managing orbital components.
/// Demonstrates Clean Architecture integration: Web → Application → Domain → Infrastructure.
/// </summary>
[Authorize]
public class OrbitalComponentsController : Controller
{
    private readonly IOrbitalComponentService _service;
    private readonly ILogger<OrbitalComponentsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OrbitalComponentsController"/>.
    /// </summary>
    public OrbitalComponentsController(
        IOrbitalComponentService service,
        ILogger<OrbitalComponentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Displays a list of all orbital components.</summary>
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var components = await _service.GetAllAsync(cancellationToken);
        return View(components);
    }

    /// <summary>Displays details for a specific orbital component.</summary>
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var component = await _service.GetByIdAsync(id, cancellationToken);
        if (component is null)
        {
            return NotFound();
        }
        return View(component);
    }

    /// <summary>Displays the form to create a new orbital component.</summary>
    public IActionResult Create()
    {
        return View(new OrbitalComponentDto());
    }

    /// <summary>Handles the POST request to create a new orbital component.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrbitalComponentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Created orbital component: {Name}.", dto.Name);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Displays the form to edit an existing orbital component.</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var component = await _service.GetByIdAsync(id, cancellationToken);
        if (component is null)
        {
            return NotFound();
        }
        return View(component);
    }

    /// <summary>Handles the POST request to update an existing orbital component.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrbitalComponentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        _logger.LogInformation("Updated orbital component ID {Id}.", id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Displays the confirmation page for deleting an orbital component.</summary>
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var component = await _service.GetByIdAsync(id, cancellationToken);
        if (component is null)
        {
            return NotFound();
        }
        return View(component);
    }

    /// <summary>Handles the POST request to confirm deletion of an orbital component.</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Deleted orbital component ID {Id}.", id);
        return RedirectToAction(nameof(Index));
    }
}
