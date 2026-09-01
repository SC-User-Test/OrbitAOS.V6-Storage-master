using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;

namespace OrbitAOS.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core repository implementation for UserProfile entity operations.
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
        public async Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }

        /// <inheritdoc />
        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
