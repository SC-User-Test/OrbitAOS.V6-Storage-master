using OrbitAOS.Domain.Entities;

namespace OrbitAOS.Domain.Interfaces;

/// <summary>
/// Repository interface for <see cref="UserProfile"/> entity operations.
/// Extends the generic repository with user-profile-specific queries.
/// </summary>
public interface IUserProfileRepository : IRepository<UserProfile>
{
    /// <summary>Retrieves a user profile by the associated ASP.NET Core Identity user ID.</summary>
    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
}
