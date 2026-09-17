using OrbitAOS.V6.Application.DTOs;

namespace OrbitAOS.V6.Application.Interfaces;

/// <summary>
/// Service interface for managing orbital components.
/// </summary>
public interface IOrbitalComponentService
{
    /// <summary>Retrieves all orbital components asynchronously.</summary>
    Task<IReadOnlyList<OrbitalComponentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single orbital component by its identifier asynchronously.</summary>
    Task<OrbitalComponentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new orbital component asynchronously.</summary>
    Task<OrbitalComponentDto> CreateAsync(OrbitalComponentDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing orbital component asynchronously.</summary>
    Task<OrbitalComponentDto?> UpdateAsync(int id, OrbitalComponentDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes an orbital component by its identifier asynchronously.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
