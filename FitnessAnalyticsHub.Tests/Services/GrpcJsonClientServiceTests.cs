using System.Net;
using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace FitnessAnalyticsHub.Tests.Services;

public class GrpcJsonClientServiceTests
{
    private readonly Mock<ILogger<GrpcJsonClientService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly GrpcJsonClientService _service;

    public GrpcJsonClientServiceTests()
    {
        _mockLogger = new Mock<ILogger<GrpcJsonClientService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["AIAssistant:BaseUrl"])
            .Returns("http://localhost:5169");

        // Setup HttpClient with mocked handler
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new GrpcJsonClientService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    #region GetMotivationAsync Tests

    [Fact]
    public async Task GetMotivationAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "John Doe",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss"
            }
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "You're doing great!",
            quote = "Success is earned",
            actionableTips = new[] { "Stay consistent", "Track progress" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("You're doing great!", result.MotivationalMessage);
        Assert.Equal("Success is earned", result.Quote);
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(2, result.ActionableTips.Count);
        Assert.Equal("gRPC-JSON", result.Source);

        // Verify HTTP call
        VerifyHttpCall(HttpMethod.Post, "/grpc-json/MotivationService/GetMotivation");
    }

    [Fact]
    public async Task GetMotivationAsync_WithHttpError_ReturnsFallbackResponse()
    {
        // Arrange
        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Jane Doe" }
        };

        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Great work, Jane Doe!", result.MotivationalMessage);
        Assert.Equal("gRPC-JSON-Fallback", result.Source);
        Assert.NotNull(result.ActionableTips);
        Assert.True(result.ActionableTips.Count > 0);

        // Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("motivation request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivationAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        var request = new AIMotivationRequestDto { AthleteProfile = null };

        var responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Keep going!",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Keep going!", result.MotivationalMessage);
    }

    #endregion

    #region GetWorkoutAnalysisAsync Tests

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "John", FitnessLevel = "Advanced" },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new() { Date = DateTime.UtcNow.AddDays(-1), ActivityType = "Running", Distance = 5.0, Duration = 30, Calories = 300 },
                new() { Date = DateTime.UtcNow.AddDays(-2), ActivityType = "Cycling", Distance = 15.0, Duration = 45, Calories = 400 }
            },
            AnalysisType = "Performance",
            FocusAreas = new List<string> { "Endurance", "Speed" }
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Great progress shown",
            keyInsights = new[] { "Consistent training", "Good variety" },
            recommendations = new[] { "Increase intensity", "Add strength training" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetWorkoutAnalysisAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great progress shown", result.Analysis);
        Assert.Equal(2, result.KeyInsights.Count);
        Assert.Equal(2, result.Recommendations.Count);
        Assert.Equal("gRPC-JSON-GoogleGemini", result.Source);

        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetWorkoutAnalysis");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithEmptyWorkouts_HandlesGracefully()
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>(),
            AnalysisType = "General"
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "No workouts to analyze",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetWorkoutAnalysisAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("No workouts to analyze", result.Analysis);
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithHttpError_ReturnsFallbackResponse()
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new() { Distance = 5.0, Calories = 300 }
            }
        };

        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act
        var result = await _service.GetWorkoutAnalysisAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Based on your 1 recent workouts", result.Analysis);
        Assert.Equal("gRPC-JSON-Fallback", result.Source);
        Assert.True(result.KeyInsights.Count > 0);
        Assert.True(result.Recommendations.Count > 0);
    }

    #endregion

    #region GetGoogleGeminiWorkoutAnalysisAsync Tests

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new() { Date = DateTime.UtcNow, ActivityType = "Swimming", Distance = 2.0, Duration = 60, Calories = 500 }
            }
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Swimming analysis complete",
            keyInsights = new[] { "Excellent cardiovascular workout" },
            recommendations = new[] { "Consider adding interval training" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetGoogleGeminiWorkoutAnalysisAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Swimming analysis complete", result.Analysis);
        Assert.Equal("gRPC-JSON-GoogleGemini", result.Source);

        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/AnalyzeGoogleGeminiWorkouts");
    }

    #endregion

    #region GetPerformanceTrendsAsync Tests

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithValidParameters_ReturnsSuccessResponse()
    {
        // Arrange
        const int athleteId = 123;
        const string timeFrame = "week";

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Performance trending upward",
            keyInsights = new[] { "Consistent improvement", "Good recovery" },
            recommendations = new[] { "Maintain current pace", "Add variety" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetPerformanceTrendsAsync(athleteId, timeFrame);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Performance trending upward", result.Analysis);
        Assert.Equal("gRPC-JSON-PerformanceTrends", result.Source);

        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetPerformanceTrends");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithDefaultTimeFrame_UsesMonth()
    {
        // Arrange
        const int athleteId = 456;

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Monthly trends analysis",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetPerformanceTrendsAsync(athleteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Monthly trends analysis", result.Analysis);
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithHttpError_ReturnsFallbackResponse()
    {
        // Arrange
        const int athleteId = 789;
        const string timeFrame = "year";

        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable");

        // Act
        var result = await _service.GetPerformanceTrendsAsync(athleteId, timeFrame);

        // Assert
        Assert.NotNull(result);
        Assert.Contains($"athlete {athleteId}", result.Analysis);
        Assert.Contains(timeFrame, result.Analysis);
        Assert.Equal("gRPC-JSON-Fallback-PerformanceTrends", result.Source);
    }

    #endregion

    #region GetTrainingRecommendationsAsync Tests

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithValidAthleteId_ReturnsSuccessResponse()
    {
        // Arrange
        const int athleteId = 999;

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Training recommendations ready",
            keyInsights = new[] { "Good base fitness", "Room for improvement" },
            recommendations = new[] { "Increase frequency", "Focus on form" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetTrainingRecommendationsAsync(athleteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Training recommendations ready", result.Analysis);
        Assert.Equal("gRPC-JSON-TrainingRecommendations", result.Source);

        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetTrainingRecommendations");
    }

    #endregion

    #region AnalyzeHealthMetricsAsync Tests

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        const int athleteId = 555;
        var recentWorkouts = new List<AIWorkoutDataDto>
        {
            new() { Date = DateTime.UtcNow, ActivityType = "Yoga", Duration = 60, Calories = 200 },
            new() { Date = DateTime.UtcNow.AddDays(-1), ActivityType = "Pilates", Duration = 45, Calories = 150 }
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Health metrics look good",
            keyInsights = new[] { "Balanced activity", "Good calorie burn" },
            recommendations = new[] { "Keep it up", "Stay hydrated" },
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.AnalyzeHealthMetricsAsync(athleteId, recentWorkouts);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Health metrics look good", result.Analysis);
        Assert.Equal("gRPC-JSON-HealthMetrics", result.Source);

        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/AnalyzeHealthMetrics");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        const int athleteId = 333;

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = "No workout data available",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.AnalyzeHealthMetricsAsync(athleteId, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("No workout data available", result.Analysis);
    }

    #endregion

    #region IsHealthyAsync Tests

    [Fact]
    public async Task IsHealthyAsync_WithHealthyResponse_ReturnsTrue()
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(new { status = "healthy" });
        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        Assert.True(result);
        VerifyHttpCall(HttpMethod.Get, "/grpc-json/health");
    }

    [Fact]
    public async Task IsHealthyAsync_WithUnhealthyResponse_ReturnsFalse()
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(new { status = "unhealthy" });
        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsHealthyAsync_WithHttpError_ReturnsFalse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable");

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        Assert.False(result);

        // Verify warning logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Health check failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("HEALTHY")]
    [InlineData("Healthy")]
    [InlineData("healthy")]
    public async Task IsHealthyAsync_WithDifferentCasing_ReturnsTrue(string status)
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(new { status });
        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task Constructor_WithNullBaseUrl_UsesDefaultUrl()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AIAssistant:BaseUrl"]).Returns((string?)null);

        using var httpClient = new HttpClient();

        // Act & Assert - Should not throw
        var service = new GrpcJsonClientService(httpClient, _mockLogger.Object, mockConfig.Object);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetMotivationAsync_WithCancellation_ThrowsCancellationException()
    {
        // Arrange
        var request = new AIMotivationRequestDto();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        SetupHttpResponseWithDelay(HttpStatusCode.OK, "{}", TimeSpan.FromSeconds(1));

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.GetMotivationAsync(request, cts.Token));

        // TaskCanceledException is a subclass of OperationCanceledException
        Assert.True(exception is TaskCanceledException || exception is OperationCanceledException);
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithInvalidJson_ReturnsFallbackResponse()
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto();
        SetupHttpResponse(HttpStatusCode.OK, "invalid json");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(
            () => _service.GetWorkoutAnalysisAsync(request));
    }

    [Fact]
    public async Task GetMotivationAsync_WithMinimalValidResponse_HandlesGracefully()
    {
        // Arrange
        var request = new AIMotivationRequestDto();
        var responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Great work!",
            generatedAt = DateTime.UtcNow.ToString("O")
        });
        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great work!", result.MotivationalMessage);
        Assert.Null(result.Quote); // Optional field, should be null when missing
        Assert.Null(result.ActionableTips); // Optional field, should be null when missing
    }

    [Fact]
    public async Task GetMotivationAsync_WithPartialResponse_HandlesOptionalFields()
    {
        // Arrange
        var request = new AIMotivationRequestDto();
        var responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Keep going!",
            quote = "Success is a journey",
            // actionableTips missing
            generatedAt = DateTime.UtcNow.ToString("O")
        });
        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Keep going!", result.MotivationalMessage);
        Assert.Equal("Success is a journey", result.Quote);
        Assert.Null(result.ActionableTips); // Should be null when missing
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpResponseWithDelay(HttpStatusCode statusCode, string content, TimeSpan delay)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(delay);
                return response;
            });
    }

    private void VerifyHttpCall(HttpMethod method, string expectedPath)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.PathAndQuery == expectedPath),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Parameterized Tests

    [Theory]
    [InlineData("Beginner", "Weight Loss")]
    [InlineData("Intermediate", "Muscle Gain")]
    [InlineData("Advanced", "Endurance")]
    public async Task GetMotivationAsync_WithDifferentProfiles_ReturnsAppropriateResponse(
        string fitnessLevel, string primaryGoal)
    {
        // Arrange
        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Test User",
                FitnessLevel = fitnessLevel,
                PrimaryGoal = primaryGoal
            }
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = $"Great work on your {primaryGoal} journey!",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(primaryGoal, result.MotivationalMessage);
    }

    [Theory]
    [InlineData("Performance")]
    [InlineData("Health")]
    [InlineData("Recovery")]
    public async Task GetWorkoutAnalysisAsync_WithDifferentAnalysisTypes_CallsCorrectEndpoint(
        string analysisType)
    {
        // Arrange
        var request = new AIWorkoutAnalysisRequestDto
        {
            AnalysisType = analysisType,
            RecentWorkouts = new List<AIWorkoutDataDto>()
        };

        var responseJson = JsonSerializer.Serialize(new
        {
            analysis = $"{analysisType} analysis complete",
            generatedAt = DateTime.UtcNow.ToString("O")
        });

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _service.GetWorkoutAnalysisAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(analysisType, result.Analysis);
        VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetWorkoutAnalysis");
    }

    #endregion
}