using OrbitAOS.Application.DTOs;

namespace OrbitAOS.Application.Interfaces;

/// <summary>
/// Service interface for user profile business logic operations.
/// Defines the application-layer contract for user profile management.
/// </summary>
public interface IUserProfileService
{
    /// <summary>Retrieves a user profile by its unique identifier.</summary>
    Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a user profile by the associated Identity user ID.</summary>
    Task<UserProfileDto?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all user profiles.</summary>
    Task<IReadOnlyList<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a new user profile.</summary>
    Task<UserProfileDto> CreateAsync(UserProfileDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing user profile.</summary>
    Task UpdateAsync(UserProfileDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user profile by its unique identifier.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
