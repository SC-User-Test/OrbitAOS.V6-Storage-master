using OrbitAOS.V6.Domain.Common;

namespace OrbitAOS.V6.Domain.Entities;

/// <summary>
/// Represents an orbital component or asset tracked by the OrbitAOS system.
/// </summary>
public class OrbitalComponent : BaseEntity
{
    /// <summary>Gets or sets the name of the orbital component.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the orbital component.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the component type classification.</summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current operational status.</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Gets or sets the orbital altitude in kilometers.</summary>
    public double? AltitudeKm { get; set; }

    /// <summary>Gets or sets the orbital inclination in degrees.</summary>
    public double? InclinationDegrees { get; set; }

    /// <summary>Gets or sets whether the component is currently active.</summary>
    public bool IsActive { get; set; } = true;
}
