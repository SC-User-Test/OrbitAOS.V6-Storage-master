using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Domain.Entities;
using OrbitAOS.V6.Infrastructure.Data;
using OrbitAOS.V6.Infrastructure.Repositories;

namespace OrbitAOS.V6.Tests.Infrastructure;

/// <summary>
/// Integration tests for <see cref="Repository{T}"/> using an in-memory database.
/// Validates that EF Core repository operations work correctly.
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<OrbitalComponent> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _repository = new Repository<OrbitalComponent>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        // Arrange
        var entity = new OrbitalComponent
        {
            Name = "Test Satellite",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true
        };

        // Act
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.OrbitalComponents.FirstOrDefaultAsync(e => e.Name == "Test Satellite");
        saved.Should().NotBeNull();
        saved!.ComponentType.Should().Be("Satellite");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var entity = new OrbitalComponent
        {
            Name = "Hubble",
            ComponentType = "Telescope",
            Status = "Active",
            IsActive = true
        };
        await _context.OrbitalComponents.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Hubble");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await _context.OrbitalComponents.AddRangeAsync(
            new OrbitalComponent { Name = "A", ComponentType = "Satellite", Status = "Active", IsActive = true },
            new OrbitalComponent { Name = "B", ComponentType = "Probe", Status = "Active", IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        await _context.OrbitalComponents.AddRangeAsync(
            new OrbitalComponent { Name = "Active1", ComponentType = "Satellite", Status = "Active", IsActive = true },
            new OrbitalComponent { Name = "Inactive1", ComponentType = "Probe", Status = "Inactive", IsActive = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(e => e.IsActive);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active1");
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        // Arrange
        var entity = new OrbitalComponent
        {
            Name = "To Delete",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true
        };
        await _context.OrbitalComponents.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(entity);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.OrbitalComponents.FindAsync(entity.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrue_WhenMatchingEntityExists()
    {
        // Arrange
        await _context.OrbitalComponents.AddAsync(
            new OrbitalComponent { Name = "ISS", ComponentType = "Station", Status = "Active", IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(e => e.Name == "ISS");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalse_WhenNoMatchingEntityExists()
    {
        // Act
        var result = await _repository.AnyAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
