using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using Xunit;

namespace OrbitAOS.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="HomeController"/>.
/// Verifies that each action returns the correct view result.
/// </summary>
public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly HomeController _controller;

    /// <summary>Initializes test fixtures.</summary>
    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_loggerMock.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResult_WithErrorViewModel()
    {
        // Arrange — HttpContext is null in unit tests; Activity.Current?.Id will be null
        // so RequestId falls back to HttpContext.TraceIdentifier which is also null here.
        // We verify the view result type and model type only.

        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(viewResult.Model);
    }

    [Fact]
    public void Error_ResponseCacheAttribute_HasNoStore()
    {
        // Arrange
        var methodInfo = typeof(HomeController).GetMethod(nameof(HomeController.Error));

        // Act
        var attr = methodInfo!
            .GetCustomAttributes(typeof(ResponseCacheAttribute), false)
            .Cast<ResponseCacheAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attr);
        Assert.True(attr.NoStore);
        Assert.Equal(0, attr.Duration);
    }
}
