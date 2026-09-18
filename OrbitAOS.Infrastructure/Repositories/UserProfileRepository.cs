using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;

namespace OrbitAOS.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserProfileRepository"/>.
/// Provides user-profile-specific data access operations.
/// </summary>
public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserProfileRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UserProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<UserProfile?> GetByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
        => await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId, cancellationToken);
}
