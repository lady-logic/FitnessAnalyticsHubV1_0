namespace FitnessAnalyticsHub.Tests.Middleware
{
    using System.Net;
    using System.Text.Json;
    using FitnessAnalyticsHub.Domain.Exceptions.Activities;
    using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
    using FitnessAnalyticsHub.Infrastructure.Exceptions;
    using FitnessAnalyticsHub.WebApi.Middleware;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Global exception handling middleware that converts domain exceptions
    /// to structured HTTP responses with appropriate status codes.
    ///
    /// This middleware ensures consistent error responses across all endpoints
    /// and eliminates the need for try-catch blocks in controllers.
    /// </summary>
    /// <remarks>
    /// Exception mapping:
    /// - ActivityNotFoundException -> 404 Not Found
    /// - InvalidStravaTokenException -> 401 Unauthorized
    /// - StravaConfigurationException -> 500 Internal Server Error
    /// - Generic exceptions -> 500 Internal Server Error
    ///
    /// All responses follow the ErrorResponse model with type, message,
    /// statusCode, details, and timestamp fields.
    /// </remarks>
    public class GlobalExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> mockLogger;
        private readonly DefaultHttpContext httpContext;

        public GlobalExceptionHandlingMiddlewareTests()
        {
            this.mockLogger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
            this.httpContext = new DefaultHttpContext();
            this.httpContext.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_WithActivityNotFoundException_ShouldReturn404()
        {
            // Arrange
            int activityId = 123;
            ActivityNotFoundException exception = new ActivityNotFoundException(activityId);

            RequestDelegate next = (HttpContext context) => throw exception;
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, this.httpContext.Response.StatusCode);
            Assert.Equal("application/json", this.httpContext.Response.ContentType);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();

            Assert.False(string.IsNullOrEmpty(responseBody));

            // Prüfen ob es gültiges JSON ist
            JsonElement errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            Assert.True(errorResponse.ValueKind == JsonValueKind.Object);
        }

        [Fact]
        public async Task InvokeAsync_WithAthleteNotFoundException_ShouldReturn404()
        {
            // Arrange
            int athleteId = 456;
            AthleteNotFoundException exception = new AthleteNotFoundException(athleteId);

            RequestDelegate next = (HttpContext context) => throw exception;
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, this.httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidStravaTokenException_ShouldReturn401()
        {
            // Arrange
            InvalidStravaTokenException exception = new InvalidStravaTokenException();

            RequestDelegate next = (HttpContext context) => throw exception;
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, this.httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithStravaConfigurationException_ShouldReturn500()
        {
            // Arrange
            StravaConfigurationException exception = new StravaConfigurationException("Missing config");

            RequestDelegate next = (HttpContext context) => throw exception;
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, this.httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_ShouldReturn500()
        {
            // Arrange
            Exception exception = new Exception("Generic error");

            RequestDelegate next = (HttpContext context) => throw exception;
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, this.httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_ShouldCallNext()
        {
            // Arrange
            bool nextCalled = false;
            RequestDelegate next = (HttpContext context) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            GlobalExceptionHandlingMiddleware middleware = new GlobalExceptionHandlingMiddleware(next, this.mockLogger.Object);

            // Act
            await middleware.InvokeAsync(this.httpContext);

            // Assert
            Assert.True(nextCalled);
        }
    }
}