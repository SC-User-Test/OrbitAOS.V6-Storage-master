using OrbitAOS.Domain.Entities;

namespace OrbitAOS.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for UserProfile entity operations.
    /// </summary>
    public interface IUserProfileRepository : IRepository<UserProfile>
    {
        /// <summary>Retrieves a user profile by the associated ASP.NET Core Identity user ID.</summary>
        Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId);

        /// <summary>Retrieves a user profile by email address.</summary>
        Task<UserProfile?> GetByEmailAsync(string email);
    }
}
