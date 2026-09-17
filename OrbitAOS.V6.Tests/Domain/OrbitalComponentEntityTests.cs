using Xunit;
using FluentAssertions;
using OrbitAOS.V6.Domain.Entities;

namespace OrbitAOS.V6.Tests.Domain;

/// <summary>
/// Unit tests for <see cref="OrbitalComponent"/> domain entity.
/// Validates entity initialization and property behavior.
/// </summary>
public class OrbitalComponentEntityTests
{
    [Fact]
    public void OrbitalComponent_ShouldHaveDefaultValues_WhenCreated()
    {
        // Act
        var entity = new OrbitalComponent();

        // Assert
        entity.Name.Should().Be(string.Empty);
        entity.ComponentType.Should().Be(string.Empty);
        entity.Status.Should().Be("Active");
        entity.IsActive.Should().BeTrue();
        entity.AltitudeKm.Should().BeNull();
        entity.InclinationDegrees.Should().BeNull();
        entity.Description.Should().BeNull();
    }

    [Fact]
    public void OrbitalComponent_ShouldSetProperties_WhenAssigned()
    {
        // Arrange & Act
        var entity = new OrbitalComponent
        {
            Id = 42,
            Name = "Hubble Space Telescope",
            Description = "NASA space telescope",
            ComponentType = "Telescope",
            Status = "Active",
            AltitudeKm = 547.0,
            InclinationDegrees = 28.47,
            IsActive = true
        };

        // Assert
        entity.Id.Should().Be(42);
        entity.Name.Should().Be("Hubble Space Telescope");
        entity.Description.Should().Be("NASA space telescope");
        entity.ComponentType.Should().Be("Telescope");
        entity.Status.Should().Be("Active");
        entity.AltitudeKm.Should().Be(547.0);
        entity.InclinationDegrees.Should().Be(28.47);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void OrbitalComponent_CreatedAt_ShouldBeSetByBaseEntity()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var entity = new OrbitalComponent();
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        entity.CreatedAt.Should().BeAfter(before);
        entity.CreatedAt.Should().BeBefore(after);
    }

    [Fact]
    public void OrbitalComponent_UpdatedAt_ShouldBeNullByDefault()
    {
        // Act
        var entity = new OrbitalComponent();

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }
}
