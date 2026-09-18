using OrbitAOS.Application.DTOs;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Domain.Interfaces;

namespace OrbitAOS.Application.Services;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> providing user profile business logic.
/// Orchestrates domain operations and maps between domain entities and DTOs.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="UserProfileService"/>.
    /// </summary>
    /// <param name="userProfileRepository">The user profile repository.</param>
    public UserProfileService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _userProfileRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        var entity = await _userProfileRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _userProfileRepository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> CreateAsync(UserProfileDto dto, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(dto);
        var created = await _userProfileRepository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserProfileDto dto, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(dto);
        entity.UpdatedAt = DateTime.UtcNow;
        await _userProfileRepository.UpdateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _userProfileRepository.DeleteAsync(id, cancellationToken);
    }

    // ── Private mapping helpers ──────────────────────────────────────────────

    private static UserProfileDto MapToDto(UserProfile entity) => new()
    {
        Id = entity.Id,
        IdentityUserId = entity.IdentityUserId,
        DisplayName = entity.DisplayName,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Department = entity.Department,
        IsActive = entity.IsActive
    };

    private static UserProfile MapToEntity(UserProfileDto dto) => new()
    {
        Id = dto.Id,
        IdentityUserId = dto.IdentityUserId,
        DisplayName = dto.DisplayName,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Department = dto.Department,
        IsActive = dto.IsActive
    };
}
