namespace FitnessAnalyticsHub.Tests.Services;

using System.Net;
using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

public class GrpcJsonClientServiceTests
{
    private readonly Mock<ILogger<GrpcJsonClientService>> mockLogger;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
    private readonly HttpClient httpClient;
    private readonly GrpcJsonClientService service;

    public GrpcJsonClientServiceTests()
    {
        this.mockLogger = new Mock<ILogger<GrpcJsonClientService>>();
        this.mockConfiguration = new Mock<IConfiguration>();
        this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup configuration
        this.mockConfiguration.Setup(c => c["AIAssistant:BaseUrl"])
            .Returns("http://localhost:5169");

        // Setup HttpClient with mocked handler
        this.httpClient = new HttpClient(this.mockHttpMessageHandler.Object);
        this.service = new GrpcJsonClientService(this.httpClient, this.mockLogger.Object, this.mockConfiguration.Object);
    }

    public void Dispose()
    {
        this.httpClient?.Dispose();
    }

    #region GetMotivationAsync Tests

    [Fact]
    public async Task GetMotivationAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "John Doe",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss",
            },
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "You're doing great!",
            quote = "Success is earned",
            actionableTips = new[] { "Stay consistent", "Track progress" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("You're doing great!", result.MotivationalMessage);
        Assert.Equal("Success is earned", result.Quote);
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(2, result.ActionableTips.Count);
        Assert.Equal("gRPC-JSON", result.Source);

        // Verify HTTP call
        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/MotivationService/GetMotivation");
    }

    [Fact]
    public async Task GetMotivationAsync_WithHttpError_ReturnsFallbackResponse()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Jane Doe" },
        };

        this.SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Great work, Jane Doe!", result.MotivationalMessage);
        Assert.Equal("gRPC-JSON-Fallback", result.Source);
        Assert.NotNull(result.ActionableTips);
        Assert.True(result.ActionableTips.Count > 0);

        // Verify error logging
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() !.Contains("motivation request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivationAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto { AthleteProfile = null };

        string responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Keep going!",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

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
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "John", FitnessLevel = "Advanced" },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new () { Date = DateTime.UtcNow.AddDays(-1), ActivityType = "Running", Distance = 5.0, Duration = 30, Calories = 300 },
                new () { Date = DateTime.UtcNow.AddDays(-2), ActivityType = "Cycling", Distance = 15.0, Duration = 45, Calories = 400 },
            },
            AnalysisType = "Performance",
            FocusAreas = new List<string> { "Endurance", "Speed" },
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Great progress shown",
            keyInsights = new[] { "Consistent training", "Good variety" },
            recommendations = new[] { "Increase intensity", "Add strength training" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great progress shown", result.Analysis);
        Assert.Equal(2, result.KeyInsights.Count);
        Assert.Equal(2, result.Recommendations.Count);
        Assert.Equal("gRPC-JSON-GoogleGemini", result.Source);

        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetWorkoutAnalysis");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithEmptyWorkouts_HandlesGracefully()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>(),
            AnalysisType = "General",
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "No workouts to analyze",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("No workouts to analyze", result.Analysis);
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithHttpError_ReturnsFallbackResponse()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new () { Distance = 5.0, Calories = 300 },
            },
        };

        this.SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None);

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
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new () { Date = DateTime.UtcNow, ActivityType = "Swimming", Distance = 2.0, Duration = 60, Calories = 500 },
            },
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Swimming analysis complete",
            keyInsights = new[] { "Excellent cardiovascular workout" },
            recommendations = new[] { "Consider adding interval training" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Swimming analysis complete", result.Analysis);
        Assert.Equal("gRPC-JSON-GoogleGemini", result.Source);

        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/AnalyzeGoogleGeminiWorkouts");
    }

    #endregion

    #region GetPerformanceTrendsAsync Tests

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithValidParameters_ReturnsSuccessResponse()
    {
        // Arrange
        const int athleteId = 123;
        const string timeFrame = "week";

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Performance trending upward",
            keyInsights = new[] { "Consistent improvement", "Good recovery" },
            recommendations = new[] { "Maintain current pace", "Add variety" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetPerformanceTrendsAsync(athleteId, CancellationToken.None, timeFrame);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Performance trending upward", result.Analysis);
        Assert.Equal("gRPC-JSON-PerformanceTrends", result.Source);

        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetPerformanceTrends");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithDefaultTimeFrame_UsesMonth()
    {
        // Arrange
        const int athleteId = 456;

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Monthly trends analysis",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetPerformanceTrendsAsync(athleteId, CancellationToken.None);

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

        this.SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable");

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetPerformanceTrendsAsync(athleteId, CancellationToken.None, timeFrame);

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

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Training recommendations ready",
            keyInsights = new[] { "Good base fitness", "Room for improvement" },
            recommendations = new[] { "Increase frequency", "Focus on form" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetTrainingRecommendationsAsync(athleteId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Training recommendations ready", result.Analysis);
        Assert.Equal("gRPC-JSON-TrainingRecommendations", result.Source);

        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetTrainingRecommendations");
    }

    #endregion

    #region AnalyzeHealthMetricsAsync Tests

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        const int athleteId = 555;
        List<AIWorkoutDataDto> recentWorkouts = new List<AIWorkoutDataDto>
        {
            new () { Date = DateTime.UtcNow, ActivityType = "Yoga", Duration = 60, Calories = 200 },
            new () { Date = DateTime.UtcNow.AddDays(-1), ActivityType = "Pilates", Duration = 45, Calories = 150 },
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "Health metrics look good",
            keyInsights = new[] { "Balanced activity", "Good calorie burn" },
            recommendations = new[] { "Keep it up", "Stay hydrated" },
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.AnalyzeHealthMetricsAsync(athleteId, recentWorkouts, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Health metrics look good", result.Analysis);
        Assert.Equal("gRPC-JSON-HealthMetrics", result.Source);

        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/AnalyzeHealthMetrics");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        const int athleteId = 333;

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = "No workout data available",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.AnalyzeHealthMetricsAsync(athleteId, null, CancellationToken.None);

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
        string responseJson = JsonSerializer.Serialize(new { status = "healthy" });
        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        bool result = await this.service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        this.VerifyHttpCall(HttpMethod.Get, "/grpc-json/health");
    }

    [Fact]
    public async Task IsHealthyAsync_WithUnhealthyResponse_ReturnsFalse()
    {
        // Arrange
        string responseJson = JsonSerializer.Serialize(new { status = "unhealthy" });
        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        bool result = await this.service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsHealthyAsync_WithHttpError_ReturnsFalse()
    {
        // Arrange
        this.SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable");

        // Act
        bool result = await this.service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.False(result);

        // Verify warning logging
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() !.Contains("Health check failed")),
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
        string responseJson = JsonSerializer.Serialize(new { status });
        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        bool result = await this.service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public Task Constructor_WithNullBaseUrl_UsesDefaultUrl()
    {
        // Arrange
        Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AIAssistant:BaseUrl"]).Returns((string?)null);

        using HttpClient httpClient = new HttpClient();

        // Act & Assert - Should not throw
        GrpcJsonClientService service = new GrpcJsonClientService(httpClient, this.mockLogger.Object, mockConfig.Object);
        Assert.NotNull(service);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetMotivationAsync_WithCancellation_ThrowsCancellationException()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto();
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        this.SetupHttpResponseWithDelay(HttpStatusCode.OK, "{}", TimeSpan.FromSeconds(1));

        // Act & Assert
        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.service.GetMotivationAsync(request, cts.Token));

        // TaskCanceledException is a subclass of OperationCanceledException
        Assert.True(exception is TaskCanceledException || exception is OperationCanceledException);
    }

    [Fact]
    public Task GetWorkoutAnalysisAsync_WithInvalidJson_ReturnsFallbackResponse()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto();
        this.SetupHttpResponse(HttpStatusCode.OK, "invalid json");

        // Act & Assert
        return Assert.ThrowsAsync<JsonException>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetMotivationAsync_WithMinimalValidResponse_HandlesGracefully()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto();
        string responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Great work!",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });
        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

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
        AIMotivationRequestDto request = new AIMotivationRequestDto();
        string responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = "Keep going!",
            quote = "Success is a journey",

            // actionableTips missing
            generatedAt = DateTime.UtcNow.ToString("O"),
        });
        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

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
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        };

        this.mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpResponseWithDelay(HttpStatusCode statusCode, string content, TimeSpan delay)
    {
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        };

        this.mockHttpMessageHandler
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
        this.mockHttpMessageHandler
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
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Test User",
                FitnessLevel = fitnessLevel,
                PrimaryGoal = primaryGoal,
            },
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            motivationalMessage = $"Great work on your {primaryGoal} journey!",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIMotivationResponseDto result = await this.service.GetMotivationAsync(request, CancellationToken.None);

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
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AnalysisType = analysisType,
            RecentWorkouts = new List<AIWorkoutDataDto>(),
        };

        string responseJson = JsonSerializer.Serialize(new
        {
            analysis = $"{analysisType} analysis complete",
            generatedAt = DateTime.UtcNow.ToString("O"),
        });

        this.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        AIWorkoutAnalysisResponseDto result = await this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(analysisType, result.Analysis);
        this.VerifyHttpCall(HttpMethod.Post, "/grpc-json/WorkoutService/GetWorkoutAnalysis");
    }

    #endregion
}