using OrbitAOS.Domain.Common;

namespace OrbitAOS.Domain.Entities;

/// <summary>
/// Represents an application user profile entity in the domain layer.
/// This entity stores additional profile information beyond ASP.NET Core Identity.
/// </summary>
public class UserProfile : BaseEntity
{
    /// <summary>Gets or sets the ASP.NET Core Identity user ID (foreign key).</summary>
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the user's department or organizational unit.</summary>
    public string? Department { get; set; }

    /// <summary>Gets or sets whether the user profile is active.</summary>
    public bool IsActive { get; set; } = true;
}
