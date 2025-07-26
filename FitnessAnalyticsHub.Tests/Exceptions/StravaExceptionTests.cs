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
            string message = "Test strava service error";

            // Act
            StravaServiceException exception = new StravaServiceException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void StravaServiceException_ShouldSetInnerException()
        {
            // Arrange
            string message = "Test error";
            Exception innerException = new Exception("Inner error");

            // Act
            StravaServiceException exception = new StravaServiceException(message, innerException);

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
            InvalidStravaTokenException exception = new InvalidStravaTokenException();

            // Assert
            Assert.Equal("Invalid or expired Strava access token", exception.Message);
        }

        [Fact]
        public void InvalidStravaTokenException_WithMessage_ShouldSetCustomMessage()
        {
            // Arrange
            string customMessage = "Custom token error message";

            // Act
            InvalidStravaTokenException exception = new InvalidStravaTokenException(customMessage);

            // Assert
            Assert.Equal(customMessage, exception.Message);
        }

        [Fact]
        public void InvalidStravaTokenException_ShouldInheritFromStravaServiceException()
        {
            // Act
            InvalidStravaTokenException exception = new InvalidStravaTokenException();

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion

        #region StravaApiException Tests

        [Fact]
        public void StravaApiException_ShouldSetMessageAndStatusCode()
        {
            // Arrange
            string message = "API error occurred";
            int statusCode = 401;

            // Act
            StravaApiException exception = new StravaApiException(message, statusCode);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
        }

        [Fact]
        public void StravaApiException_ShouldInheritFromStravaServiceException()
        {
            // Act
            StravaApiException exception = new StravaApiException("Error", 500);

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
            string message = "Test API error";

            // Act
            StravaApiException exception = new StravaApiException(message, statusCode);

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
            string originalMessage = "Missing client ID";

            // Act
            StravaConfigurationException exception = new StravaConfigurationException(originalMessage);

            // Assert
            Assert.Equal("Strava configuration error: Missing client ID", exception.Message);
        }

        [Fact]
        public void StravaConfigurationException_ShouldInheritFromStravaServiceException()
        {
            // Act
            StravaConfigurationException exception = new StravaConfigurationException("Config error");

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion

        #region StravaAuthorizationException Tests

        [Fact]
        public void StravaAuthorizationException_ShouldPrefixMessage()
        {
            // Arrange
            string originalMessage = "No auth code provided";

            // Act
            StravaAuthorizationException exception = new StravaAuthorizationException(originalMessage);

            // Assert
            Assert.Equal("Strava authorization error: No auth code provided", exception.Message);
        }

        [Fact]
        public void StravaAuthorizationException_ShouldInheritFromStravaServiceException()
        {
            // Act
            StravaAuthorizationException exception = new StravaAuthorizationException("Auth error");

            // Assert
            Assert.IsAssignableFrom<StravaServiceException>(exception);
        }

        #endregion
    }
}