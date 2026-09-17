namespace OrbitAOS.V6.Application.DTOs;

/// <summary>
/// Data Transfer Object representing an orbital component for application layer communication.
/// </summary>
public class OrbitalComponentDto
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the component name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the component description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the component type.</summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the operational status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the orbital altitude in kilometers.</summary>
    public double? AltitudeKm { get; set; }

    /// <summary>Gets or sets the orbital inclination in degrees.</summary>
    public double? InclinationDegrees { get; set; }

    /// <summary>Gets or sets whether the component is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime? UpdatedAt { get; set; }
}
