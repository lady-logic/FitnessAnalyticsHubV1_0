namespace FitnessAnalyticsHub.Tests.Exceptions
{
    using FitnessAnalyticsHub.Infrastructure.Exceptions;

    public class StravaExceptionTests
    {
        #region StravaServiceException Tests

        [Fact]
        public void StravaServiceException_ShouldSetCorrectMessage()
        {
            // Arrange
            var message = "Test strava service error";

            // Act
            var exception = new StravaServiceException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void StravaServiceException_ShouldSetInnerException()
        {
            // Arrange
            var message = "Test error";
            var innerException = new Exception("Inner error");

            // Act
            var exception = new StravaServiceException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        #endregion

        #region InvalidStravaTokenException Tests

        [Fact]
        public void InvalidStravaTokenException_DefaultConstructor_ShouldSetDefaultMessage()
        {
            // Act
            var exception = new InvalidStravaTokenException();

            // Assert
            Assert.Equal("Invalid or expired Strava access token", exception.Message);
        }

        [Fact]
        public void InvalidStravaTokenException_WithMessage_ShouldSetCustomMessage()
        {
            // Arrange
            var customMessage = "Custom token error message";

            // Act
            var exception = new InvalidStravaTokenException(customMessage);

            // Assert
            Assert.Equal(customMessage, exception.Message);
        }

        [Fact]
        public void InvalidStravaTokenException_ShouldInheritFromStravaServiceException()
        {
            // Act
            var exception = new InvalidStravaTokenException();

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion

        #region StravaApiException Tests

        [Fact]
        public void StravaApiException_ShouldSetMessageAndStatusCode()
        {
            // Arrange
            var message = "API error occurred";
            var statusCode = 401;

            // Act
            var exception = new StravaApiException(message, statusCode);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
        }

        [Fact]
        public void StravaApiException_ShouldInheritFromStravaServiceException()
        {
            // Act
            var exception = new StravaApiException("Error", 500);

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(403)]
        [InlineData(429)]
        [InlineData(500)]
        public void StravaApiException_ShouldWorkWithDifferentStatusCodes(int statusCode)
        {
            // Arrange
            var message = "Test API error";

            // Act
            var exception = new StravaApiException(message, statusCode);

            // Assert
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Equal(message, exception.Message);
        }

        #endregion

        #region StravaConfigurationException Tests

        [Fact]
        public void StravaConfigurationException_ShouldPrefixMessage()
        {
            // Arrange
            var originalMessage = "Missing client ID";

            // Act
            var exception = new StravaConfigurationException(originalMessage);

            // Assert
            Assert.Equal("Strava configuration error: Missing client ID", exception.Message);
        }

        [Fact]
        public void StravaConfigurationException_ShouldInheritFromStravaServiceException()
        {
            // Act
            var exception = new StravaConfigurationException("Config error");

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion

        #region StravaAuthorizationException Tests

        [Fact]
        public void StravaAuthorizationException_ShouldPrefixMessage()
        {
            // Arrange
            var originalMessage = "No auth code provided";

            // Act
            var exception = new StravaAuthorizationException(originalMessage);

            // Assert
            Assert.Equal("Strava authorization error: No auth code provided", exception.Message);
        }

        [Fact]
        public void StravaAuthorizationException_ShouldInheritFromStravaServiceException()
        {
            // Act
            var exception = new StravaAuthorizationException("Auth error");

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion
    }
}