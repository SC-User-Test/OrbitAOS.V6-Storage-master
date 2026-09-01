using OrbitAOS.Domain.Common;

namespace OrbitAOS.Domain.Entities
{
    /// <summary>
    /// Represents an application user profile linked to ASP.NET Core Identity.
    /// </summary>
    public class UserProfile : BaseEntity
    {
        /// <summary>Gets or sets the ASP.NET Core Identity user ID (foreign key).</summary>
        public string IdentityUserId { get; set; } = string.Empty;

        /// <summary>Gets or sets the display name of the user.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the user profile is active.</summary>
        public bool IsActive { get; set; } = true;
    }
}
