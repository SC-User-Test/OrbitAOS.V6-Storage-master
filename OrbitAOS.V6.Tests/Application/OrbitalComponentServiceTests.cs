using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Services;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Domain.Interfaces;

namespace OrbitAOS.V6.Tests.Application;

/// <summary>
/// Unit tests for <see cref="OrbitalComponentService"/>.
/// Tests business logic in isolation using mocked dependencies.
/// </summary>
public class OrbitalComponentServiceTests
{
    private readonly Mock<IRepository<OrbitalComponent>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrbitalComponentService>> _loggerMock;
    private readonly OrbitalComponentService _sut;

    public OrbitalComponentServiceTests()
    {
        _repositoryMock = new Mock<IRepository<OrbitalComponent>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrbitalComponentService>>();
        _sut = new OrbitalComponentService(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllComponents_WhenComponentsExist()
    {
        // Arrange
        var entities = new List<OrbitalComponent>
        {
            new() { Id = 1, Name = "Hubble", ComponentType = "Telescope", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "ISS", ComponentType = "Station", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities.AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Hubble");
        result.Should().Contain(c => c.Name == "ISS");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoComponentsExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrbitalComponent>().AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnComponent_WhenComponentExists()
    {
        // Arrange
        var entity = new OrbitalComponent
        {
            Id = 1,
            Name = "Hubble",
            ComponentType = "Telescope",
            Status = "Active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Hubble");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenComponentDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponent?)null);

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnComponent()
    {
        // Arrange
        var dto = new OrbitalComponentDto
        {
            Name = "New Satellite",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true,
            AltitudeKm = 550.0,
            InclinationDegrees = 53.0
        };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<OrbitalComponent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponent entity, CancellationToken _) =>
            {
                entity.Id = 10;
                return entity;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Satellite");
        result.ComponentType.Should().Be("Satellite");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<OrbitalComponent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnComponent_WhenComponentExists()
    {
        // Arrange
        var existing = new OrbitalComponent
        {
            Id = 1,
            Name = "Old Name",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var updateDto = new OrbitalComponentDto
        {
            Name = "Updated Name",
            ComponentType = "Probe",
            Status = "Maintenance",
            IsActive = false
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.ComponentType.Should().Be("Probe");
        result.Status.Should().Be("Maintenance");
        result.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(It.IsAny<OrbitalComponent>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenComponentDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponent?)null);

        // Act
        var result = await _sut.UpdateAsync(999, new OrbitalComponentDto());

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.Update(It.IsAny<OrbitalComponent>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenComponentExists()
    {
        // Arrange
        var entity = new OrbitalComponent
        {
            Id = 1,
            Name = "To Delete",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(It.IsAny<OrbitalComponent>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenComponentDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponent?)null);

        // Act
        var result = await _sut.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.Remove(It.IsAny<OrbitalComponent>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
