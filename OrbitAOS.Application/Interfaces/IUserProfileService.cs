using OrbitAOS.Application.DTOs;

namespace OrbitAOS.Application.Interfaces
{
    /// <summary>
    /// Service interface for user profile business logic operations.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>Retrieves a user profile by its identifier.</summary>
        Task<UserProfileDto?> GetByIdAsync(int id);

        /// <summary>Retrieves all user profiles.</summary>
        Task<IEnumerable<UserProfileDto>> GetAllAsync();

        /// <summary>Retrieves a user profile by the associated Identity user ID.</summary>
        Task<UserProfileDto?> GetByIdentityUserIdAsync(string identityUserId);

        /// <summary>Creates a new user profile.</summary>
        Task<UserProfileDto> CreateAsync(UserProfileDto dto);

        /// <summary>Updates an existing user profile.</summary>
        Task UpdateAsync(UserProfileDto dto);

        /// <summary>Deletes a user profile by its identifier.</summary>
        Task DeleteAsync(int id);
    }
}
