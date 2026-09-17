using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Web.Controllers;

namespace OrbitAOS.V6.Tests.Web;

/// <summary>
/// Unit tests for <see cref="OrbitalComponentsController"/>.
/// Tests controller actions in isolation using mocked service dependencies.
/// </summary>
public class OrbitalComponentsControllerTests
{
    private readonly Mock<IOrbitalComponentService> _serviceMock;
    private readonly Mock<ILogger<OrbitalComponentsController>> _loggerMock;
    private readonly OrbitalComponentsController _sut;

    public OrbitalComponentsControllerTests()
    {
        _serviceMock = new Mock<IOrbitalComponentService>();
        _loggerMock = new Mock<ILogger<OrbitalComponentsController>>();
        _sut = new OrbitalComponentsController(_serviceMock.Object, _loggerMock.Object);

        // Set up a default HttpContext for the controller
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithComponents()
    {
        // Arrange
        var components = new List<OrbitalComponentDto>
        {
            new() { Id = 1, Name = "Hubble", ComponentType = "Telescope", Status = "Active", IsActive = true },
            new() { Id = 2, Name = "ISS", ComponentType = "Station", Status = "Active", IsActive = true }
        }.AsReadOnly();

        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(components);

        // Act
        var result = await _sut.Index(CancellationToken.None);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IReadOnlyList<OrbitalComponentDto>>().Subject;
        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task Details_ShouldReturnView_WhenComponentExists()
    {
        // Arrange
        var component = new OrbitalComponentDto { Id = 1, Name = "Hubble", ComponentType = "Telescope", Status = "Active", IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        // Act
        var result = await _sut.Details(1, CancellationToken.None);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<OrbitalComponentDto>().Subject;
        model.Name.Should().Be("Hubble");
    }

    [Fact]
    public async Task Details_ShouldReturnNotFound_WhenComponentDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponentDto?)null);

        // Act
        var result = await _sut.Details(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Create_GET_ShouldReturnViewWithEmptyDto()
    {
        // Act
        var result = _sut.Create();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<OrbitalComponentDto>();
    }

    [Fact]
    public async Task Create_POST_ShouldRedirectToIndex_WhenModelIsValid()
    {
        // Arrange
        var dto = new OrbitalComponentDto
        {
            Name = "New Satellite",
            ComponentType = "Satellite",
            Status = "Active",
            IsActive = true
        };
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrbitalComponentDto { Id = 5, Name = "New Satellite", ComponentType = "Satellite", Status = "Active", IsActive = true });

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_POST_ShouldReturnView_WhenModelIsInvalid()
    {
        // Arrange
        var dto = new OrbitalComponentDto();
        _sut.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_GET_ShouldReturnView_WhenComponentExists()
    {
        // Arrange
        var component = new OrbitalComponentDto { Id = 1, Name = "Hubble", ComponentType = "Telescope", Status = "Active", IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        // Act
        var result = await _sut.Edit(1, CancellationToken.None);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<OrbitalComponentDto>().Subject;
        model.Id.Should().Be(1);
    }

    [Fact]
    public async Task Edit_GET_ShouldReturnNotFound_WhenComponentDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrbitalComponentDto?)null);

        // Act
        var result = await _sut.Edit(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_GET_ShouldReturnView_WhenComponentExists()
    {
        // Arrange
        var component = new OrbitalComponentDto { Id = 1, Name = "Hubble", ComponentType = "Telescope", Status = "Active", IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        // Act
        var result = await _sut.Delete(1, CancellationToken.None);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<OrbitalComponentDto>();
    }

    [Fact]
    public async Task DeleteConfirmed_ShouldRedirectToIndex()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteConfirmed(1, CancellationToken.None);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }
}
