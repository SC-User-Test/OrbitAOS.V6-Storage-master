using Microsoft.EntityFrameworkCore;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Infrastructure.Repositories;
using Xunit;

namespace OrbitAOS.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="UserProfileRepository"/> using EF Core InMemory provider.
/// Verifies CRUD operations and the identity-user-specific query.
/// </summary>
public class UserProfileRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserProfileRepository _repository;

    /// <summary>Initializes an in-memory database context for each test.</summary>
    public UserProfileRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserProfileRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        // Arrange
        var profile = new UserProfile
        {
            IdentityUserId = "user-001",
            DisplayName = "Test User",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(profile);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Test User", result.DisplayName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var profile = new UserProfile
        {
            IdentityUserId = "user-002",
            DisplayName = "Another User",
            IsActive = true
        };
        await _repository.AddAsync(profile);

        // Act
        var result = await _repository.GetByIdAsync(profile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Another User", result.DisplayName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await _repository.AddAsync(new UserProfile { IdentityUserId = "u1", DisplayName = "User 1", IsActive = true });
        await _repository.AddAsync(new UserProfile { IdentityUserId = "u2", DisplayName = "User 2", IsActive = true });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyEntity()
    {
        // Arrange
        var profile = new UserProfile
        {
            IdentityUserId = "user-003",
            DisplayName = "Original Name",
            IsActive = true
        };
        await _repository.AddAsync(profile);

        // Act
        profile.DisplayName = "Updated Name";
        await _repository.UpdateAsync(profile);
        var updated = await _repository.GetByIdAsync(profile.Id);

        // Assert
        Assert.Equal("Updated Name", updated?.DisplayName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var profile = new UserProfile
        {
            IdentityUserId = "user-004",
            DisplayName = "To Delete",
            IsActive = true
        };
        await _repository.AddAsync(profile);

        // Act
        await _repository.DeleteAsync(profile.Id);
        var deleted = await _repository.GetByIdAsync(profile.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetByIdentityUserIdAsync_ShouldReturnCorrectProfile()
    {
        // Arrange
        await _repository.AddAsync(new UserProfile
        {
            IdentityUserId = "identity-abc",
            DisplayName = "Identity User",
            IsActive = true
        });

        // Act
        var result = await _repository.GetByIdentityUserIdAsync("identity-abc");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Identity User", result.DisplayName);
    }

    [Fact]
    public async Task GetByIdentityUserIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _repository.GetByIdentityUserIdAsync("nonexistent-id");

        // Assert
        Assert.Null(result);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
