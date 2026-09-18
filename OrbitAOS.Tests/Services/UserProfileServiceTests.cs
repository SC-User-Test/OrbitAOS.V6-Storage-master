using Moq;
using OrbitAOS.Application.DTOs;
using OrbitAOS.Application.Services;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Domain.Interfaces;
using Xunit;

namespace OrbitAOS.Tests.Services;

/// <summary>
/// Unit tests for <see cref="UserProfileService"/>.
/// Uses Moq to isolate the service from the repository layer.
/// </summary>
public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _repositoryMock;
    private readonly UserProfileService _service;

    /// <summary>Initializes mocks and the service under test.</summary>
    public UserProfileServiceTests()
    {
        _repositoryMock = new Mock<IUserProfileRepository>();
        _service = new UserProfileService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenEntityExists()
    {
        // Arrange
        var entity = new UserProfile
        {
            Id = 1,
            IdentityUserId = "user-001",
            DisplayName = "Test User",
            IsActive = true
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test User", result.DisplayName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((UserProfile?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var entities = new List<UserProfile>
        {
            new() { Id = 1, IdentityUserId = "u1", DisplayName = "User 1", IsActive = true },
            new() { Id = 2, IdentityUserId = "u2", DisplayName = "User 2", IsActive = false }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(entities.AsReadOnly());

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("User 1", results[0].DisplayName);
        Assert.Equal("User 2", results[1].DisplayName);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryAddAndReturnDto()
    {
        // Arrange
        var dto = new UserProfileDto
        {
            IdentityUserId = "user-new",
            DisplayName = "New User",
            IsActive = true
        };
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<UserProfile>(), default))
            .ReturnsAsync((UserProfile p, CancellationToken _) =>
            {
                p.Id = 42;
                return p;
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal(42, result.Id);
        Assert.Equal("New User", result.DisplayName);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepositoryUpdate()
    {
        // Arrange
        var dto = new UserProfileDto { Id = 5, IdentityUserId = "u5", DisplayName = "Updated", IsActive = true };

        // Act
        await _service.UpdateAsync(dto);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Act
        await _service.DeleteAsync(7);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(7, default), Times.Once);
    }

    [Fact]
    public async Task GetByIdentityUserIdAsync_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var entity = new UserProfile
        {
            Id = 10,
            IdentityUserId = "identity-xyz",
            DisplayName = "Identity User",
            IsActive = true
        };
        _repositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync("identity-xyz", default))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdentityUserIdAsync("identity-xyz");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Identity User", result.DisplayName);
    }
}
