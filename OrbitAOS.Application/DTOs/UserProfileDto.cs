namespace OrbitAOS.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a user profile for use in the application layer.
/// </summary>
public class UserProfileDto
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ASP.NET Core Identity user ID.</summary>
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the user's department.</summary>
    public string? Department { get; set; }

    /// <summary>Gets or sets whether the user profile is active.</summary>
    public bool IsActive { get; set; }
}
