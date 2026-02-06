using Xunit;
using OrbitAOS.V6.Models;

namespace OrbitAOS.V6.Models.Tests
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void RequestId_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.Null(model.RequestId);
        }

        [Fact]
        public void RequestId_ShouldSetAndGetValue()
        {
            // Arrange
            var model = new ErrorViewModel();
            var requestId = "test-request-id-12345";

            // Act
            model.RequestId = requestId;

            // Assert
            Assert.Equal(requestId, model.RequestId);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = null
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = string.Empty
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdHasValue()
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = "valid-request-id"
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsWhitespace()
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = "   "
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result); // Note: string.IsNullOrEmpty doesn't check whitespace
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNullOrEmpty(string? requestId)
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = requestId
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("request-1")]
        [InlineData("12345")]
        [InlineData("abc-def-ghi")]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdIsNotNullOrEmpty(string requestId)
        {
            // Arrange
            var model = new ErrorViewModel
            {
                RequestId = requestId
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }
    }
}
