using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Infrastructure.Exceptions;
using FitnessAnalyticsHub.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace FitnessAnalyticsHub.Tests.Middleware
{
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
        private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _mockLogger;
        private readonly DefaultHttpContext _httpContext;

        public GlobalExceptionHandlingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_WithActivityNotFoundException_ShouldReturn404()
        {
            // Arrange
            var activityId = 123;
            var exception = new ActivityNotFoundException(activityId);

            RequestDelegate next = (HttpContext context) => throw exception;
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
            Assert.Equal("application/json", _httpContext.Response.ContentType);

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

            Assert.False(string.IsNullOrEmpty(responseBody));
            // Prüfen ob es gültiges JSON ist
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            Assert.True(errorResponse.ValueKind == JsonValueKind.Object);
        }

        [Fact]
        public async Task InvokeAsync_WithAthleteNotFoundException_ShouldReturn404()
        {
            // Arrange
            var athleteId = 456;
            var exception = new AthleteNotFoundException(athleteId);

            RequestDelegate next = (HttpContext context) => throw exception;
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidStravaTokenException_ShouldReturn401()
        {
            // Arrange
            var exception = new InvalidStravaTokenException();

            RequestDelegate next = (HttpContext context) => throw exception;
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithStravaConfigurationException_ShouldReturn500()
        {
            // Arrange
            var exception = new StravaConfigurationException("Missing config");

            RequestDelegate next = (HttpContext context) => throw exception;
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_ShouldReturn500()
        {
            // Arrange
            var exception = new Exception("Generic error");

            RequestDelegate next = (HttpContext context) => throw exception;
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_ShouldCallNext()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (HttpContext context) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.True(nextCalled);
        }
    }
}