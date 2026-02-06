using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace OrbitAOS.V6.Controllers.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WithLogger()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>().Object;

            // Act
            var controller = new HomeController(logger);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_ShouldAcceptLogger()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>().Object;

            // Act
            var controller = new HomeController(logger);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ShouldReturnViewResult_WithNoModel()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Model);
        }

        [Fact]
        public void Privacy_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ShouldReturnViewResult_WithNoModel()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Model);
        }

        [Fact]
        public void Error_ShouldReturnViewResult()
        {
            // Arrange
            SetupHttpContext();

            // Act
            var result = _controller.Error();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ShouldReturnViewResult_WithErrorViewModel()
        {
            // Arrange
            SetupHttpContext();

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ShouldSetRequestId_FromActivity_WhenActivityCurrentIsNotNull()
        {
            // Arrange
            SetupHttpContext();
            var activity = new Activity("test-activity");
            activity.Start();

            try
            {
                // Act
                var result = _controller.Error() as ViewResult;
                var model = result?.Model as ErrorViewModel;

                // Assert
                Assert.NotNull(model);
                Assert.NotNull(model.RequestId);
                Assert.Equal(Activity.Current?.Id, model.RequestId);
            }
            finally
            {
                activity.Stop();
            }
        }

        [Fact]
        public void Error_ShouldSetRequestId_FromTraceIdentifier_WhenActivityCurrentIsNull()
        {
            // Arrange
            var traceIdentifier = "test-trace-id-12345";
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = traceIdentifier;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Ensure no Activity.Current
            Activity.Current?.Stop();

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.NotNull(model.RequestId);
            Assert.Equal(traceIdentifier, model.RequestId);
        }

        [Fact]
        public void Error_ShouldHaveResponseCacheAttribute()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod("Error");

            // Act
            var attributes = methodInfo?.GetCustomAttributes(typeof(ResponseCacheAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);
            var attribute = attributes[0] as ResponseCacheAttribute;
            Assert.NotNull(attribute);
            Assert.Equal(0, attribute.Duration);
            Assert.Equal(ResponseCacheLocation.None, attribute.Location);
            Assert.True(attribute.NoStore);
        }

        [Fact]
        public void Error_Model_ShouldShowRequestId_WhenRequestIdIsSet()
        {
            // Arrange
            SetupHttpContext();

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void HomeController_ShouldInheritFromController()
        {
            // Assert
            Assert.IsAssignableFrom<Controller>(_controller);
        }

        [Fact]
        public void Index_ShouldReturnSameViewName()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view name
        }

        [Fact]
        public void Privacy_ShouldReturnSameViewName()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view name
        }

        private void SetupHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-identifier";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
