using Microsoft.Extensions.Logging;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Domain.Interfaces;

namespace OrbitAOS.V6.Application.Services;

/// <summary>
/// Service implementation for managing orbital components.
/// Encapsulates business logic and coordinates with the domain layer.
/// </summary>
public class OrbitalComponentService : IOrbitalComponentService
{
    private readonly IRepository<OrbitalComponent> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrbitalComponentService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OrbitalComponentService"/>.
    /// </summary>
    public OrbitalComponentService(
        IRepository<OrbitalComponent> repository,
        IUnitOfWork unitOfWork,
        ILogger<OrbitalComponentService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrbitalComponentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all orbital components.");
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<OrbitalComponentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving orbital component with ID {Id}.", id);
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<OrbitalComponentDto> CreateAsync(OrbitalComponentDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new orbital component: {Name}.", dto.Name);
        var entity = MapToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(created);
    }

    /// <inheritdoc />
    public async Task<OrbitalComponentDto?> UpdateAsync(int id, OrbitalComponentDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating orbital component with ID {Id}.", id);
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Orbital component with ID {Id} not found for update.", id);
            return null;
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ComponentType = dto.ComponentType;
        entity.Status = dto.Status;
        entity.AltitudeKm = dto.AltitudeKm;
        entity.InclinationDegrees = dto.InclinationDegrees;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting orbital component with ID {Id}.", id);
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Orbital component with ID {Id} not found for deletion.", id);
            return false;
        }

        _repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static OrbitalComponentDto MapToDto(OrbitalComponent entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        ComponentType = entity.ComponentType,
        Status = entity.Status,
        AltitudeKm = entity.AltitudeKm,
        InclinationDegrees = entity.InclinationDegrees,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static OrbitalComponent MapToEntity(OrbitalComponentDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        ComponentType = dto.ComponentType,
        Status = dto.Status,
        AltitudeKm = dto.AltitudeKm,
        InclinationDegrees = dto.InclinationDegrees,
        IsActive = dto.IsActive
    };
}
