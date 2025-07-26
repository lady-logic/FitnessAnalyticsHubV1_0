namespace FitnessAnalyticsHub.Tests.Controllers;

using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Infrastructure.Exceptions;
using FitnessAnalyticsHub.Tests.Base;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class AIControllerTests : ControllerTestBase<AIController>
{
    private readonly Mock<IAIAssistantClientService> mockAIAssistantClient;
    private readonly Mock<ILogger<AIController>> mockLogger;
    private readonly AIController controller;

    public AIControllerTests()
    {
        this.mockAIAssistantClient = new Mock<IAIAssistantClientService>();
        this.mockLogger = new Mock<ILogger<AIController>>();
        this.controller = new AIController(this.mockAIAssistantClient.Object, this.mockLogger.Object);
    }

    #region GetMotivation Tests

    [Fact]
    public async Task GetMotivation_WithValidRequest_ReturnsOkWithMotivationResponse()
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
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto
                {
                    Date = DateTime.Now.AddDays(-1),
                    ActivityType = "Run",
                    Distance = 5.0,
                    Duration = 1800,
                    Calories = 350,
                },
            },
            PreferredTone = "Encouraging",
            ContextualInfo = "Feeling a bit tired today",
        };

        AIMotivationResponseDto expectedResponse = new AIMotivationResponseDto
        {
            MotivationalMessage = "Great job on your recent run, John! Keep pushing forward!",
            Quote = "The only bad workout is the one that didn't happen.",
            ActionableTips = new List<string>
            {
                "Focus on hydration before your next run",
                "Try a 5-minute warm-up routine",
                "Set a small goal for tomorrow",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-HuggingFace",
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetMotivationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<AIMotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        AIMotivationResponseDto response = Assert.IsType<AIMotivationResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.MotivationalMessage, response.MotivationalMessage);
        Assert.Equal(expectedResponse.Quote, response.Quote);
        Assert.Equal(expectedResponse.ActionableTips.Count, response.ActionableTips?.Count);
        Assert.Equal(expectedResponse.Source, response.Source);

        // Verify that the service was called correctly
        this.mockAIAssistantClient.Verify(x => x.GetMotivationAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMotivation_WithMinimalRequest_ReturnsOkWithMotivationResponse()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Jane Smith",
                FitnessLevel = "Beginner",
                PrimaryGoal = "General Fitness",
            },

            // No recent workouts, no preferred tone, no contextual info
        };

        AIMotivationResponseDto expectedResponse = new AIMotivationResponseDto
        {
            MotivationalMessage = "You're doing amazing, Jane! Every step counts on your fitness journey.",
            Quote = "A journey of a thousand miles begins with a single step.",
            ActionableTips = new List<string>
            {
                "Start with 10 minutes of walking daily",
                "Set realistic weekly goals",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-GoogleGemini",
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetMotivationAsync(It.IsAny<AIMotivationRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<AIMotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        AIMotivationResponseDto response = Assert.IsType<AIMotivationResponseDto>(okResult.Value);

        Assert.NotNull(response.MotivationalMessage);
        Assert.NotEmpty(response.MotivationalMessage);
        Assert.Equal(expectedResponse.Source, response.Source);
    }

    [Fact]
    public async Task GetMotivation_WithAIServiceFailure_ThrowsException()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Test User",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Endurance",
            },
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetMotivationAsync(It.IsAny<AIMotivationRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AIAssistantApiException("AI service is temporarily unavailable", 503));

        // Act & Assert
        await Assert.ThrowsAsync<AIAssistantApiException>(
            () => this.controller.GetMotivation(request, CancellationToken.None));

        // Verify service was called
        this.mockAIAssistantClient.Verify(x => x.GetMotivationAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetWorkoutAnalysis Tests

    [Fact]
    public async Task GetWorkoutAnalysis_WithValidRequest_ReturnsOkWithAnalysisResponse()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Mike Johnson",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance Improvement",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto
                {
                    Date = DateTime.Now.AddDays(-1),
                    ActivityType = "Run",
                    Distance = 10.0,
                    Duration = 2700, // 45 minutes
                    Calories = 650,
                },
                new AIWorkoutDataDto
                {
                    Date = DateTime.Now.AddDays(-3),
                    ActivityType = "Ride",
                    Distance = 30.0,
                    Duration = 5400, // 90 minutes
                    Calories = 1200,
                },
            },
            AnalysisType = "Performance",
            FocusAreas = new List<string> { "endurance", "pacing", "recovery" },
        };

        AIWorkoutAnalysisResponseDto expectedResponse = new AIWorkoutAnalysisResponseDto
        {
            Analysis = "Your recent workouts show excellent endurance progression. The 10K run demonstrates improved pacing consistency compared to previous sessions.",
            KeyInsights = new List<string>
            {
                "Average pace has improved by 15 seconds per kilometer",
                "Heart rate zones indicate optimal training intensity",
                "Recovery time between high-intensity sessions is appropriate",
            },
            Recommendations = new List<string>
            {
                "Continue current endurance base building",
                "Add one tempo run per week",
                "Consider incorporating strength training 2x per week",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-GoogleGemini",
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetWorkoutAnalysisAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<AIWorkoutAnalysisResponseDto> result = await this.controller.GetWorkoutAnalysis(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        AIWorkoutAnalysisResponseDto response = Assert.IsType<AIWorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);
        Assert.Equal(expectedResponse.KeyInsights.Count, response.KeyInsights.Count);
        Assert.Equal(expectedResponse.Recommendations.Count, response.Recommendations.Count);
        Assert.Equal(expectedResponse.Source, response.Source);

        // Verify service was called correctly
        this.mockAIAssistantClient.Verify(x => x.GetWorkoutAnalysisAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutAnalysis_WithNoWorkouts_ReturnsOkWithBasicAnalysis()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "New User",
                FitnessLevel = "Beginner",
                PrimaryGoal = "Get Started",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>(), // Empty list
            AnalysisType = "General",
        };

        AIWorkoutAnalysisResponseDto expectedResponse = new AIWorkoutAnalysisResponseDto
        {
            Analysis = "Welcome to your fitness journey! Since you're just getting started, focus on building consistent habits.",
            KeyInsights = new List<string>
            {
                "Starting with 3 workouts per week is ideal for beginners",
                "Focus on form over intensity initially",
            },
            Recommendations = new List<string>
            {
                "Begin with 20-30 minute walks",
                "Add bodyweight exercises 2x per week",
                "Track your progress to stay motivated",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-Fallback",
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetWorkoutAnalysisAsync(It.IsAny<AIWorkoutAnalysisRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<AIWorkoutAnalysisResponseDto> result = await this.controller.GetWorkoutAnalysis(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        AIWorkoutAnalysisResponseDto response = Assert.IsType<AIWorkoutAnalysisResponseDto>(okResult.Value);

        Assert.NotNull(response.Analysis);
        Assert.NotEmpty(response.Analysis);
        Assert.NotNull(response.KeyInsights);
        Assert.NotNull(response.Recommendations);
    }

    [Theory]
    [InlineData("Performance")]
    [InlineData("Health")]
    [InlineData("Trends")]
    [InlineData("General")]
    public async Task GetWorkoutAnalysis_WithDifferentAnalysisTypes_ReturnsOkWithAppropriateResponse(string analysisType)
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Test Athlete",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Endurance",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto
                {
                    Date = DateTime.Now.AddDays(-1),
                    ActivityType = "Run",
                    Distance = 5.0,
                    Duration = 1800,
                    Calories = 400,
                },
            },
            AnalysisType = analysisType,
        };

        AIWorkoutAnalysisResponseDto expectedResponse = new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Analysis focused on {analysisType.ToLower()} aspects of your training.",
            KeyInsights = new List<string> { $"Key insight for {analysisType} analysis" },
            Recommendations = new List<string> { $"Recommendation based on {analysisType} focus" },
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-Test",
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetWorkoutAnalysisAsync(It.IsAny<AIWorkoutAnalysisRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<AIWorkoutAnalysisResponseDto> result = await this.controller.GetWorkoutAnalysis(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        AIWorkoutAnalysisResponseDto response = Assert.IsType<AIWorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Contains(analysisType.ToLower(), response.Analysis.ToLower());

        // Verify the request was passed correctly
        this.mockAIAssistantClient.Verify(
            x => x.GetWorkoutAnalysisAsync(
            It.Is<AIWorkoutAnalysisRequestDto>(r => r.AnalysisType == analysisType),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAIHealth Tests

    [Fact]
    public async Task GetAIHealth_WhenServiceIsHealthy_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        this.mockAIAssistantClient
            .Setup(x => x.IsHealthyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        ActionResult<object> result = await this.controller.GetAIHealth(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        object? response = okResult.Value;

        // Use reflection to check the anonymous object properties
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? isHealthyProperty = responseType.GetProperty("isHealthy");
        System.Reflection.PropertyInfo? serviceProperty = responseType.GetProperty("service");
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? timestampProperty = responseType.GetProperty("timestamp");

        Assert.NotNull(isHealthyProperty);
        Assert.NotNull(serviceProperty);
        Assert.NotNull(statusProperty);
        Assert.NotNull(timestampProperty);

        Assert.True((bool)isHealthyProperty.GetValue(response) !);
        Assert.Equal("AIAssistant", serviceProperty.GetValue(response));
        Assert.Equal("Available", statusProperty.GetValue(response));

        DateTime timestamp = (DateTime)timestampProperty.GetValue(response) !;
        Assert.True(DateTime.UtcNow.Subtract(timestamp).TotalSeconds < 5); // Within 5 seconds

        // Verify service was called
        this.mockAIAssistantClient.Verify(x => x.IsHealthyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAIHealth_WhenServiceIsUnhealthy_ReturnsOkWithUnhealthyStatus()
    {
        // Arrange
        this.mockAIAssistantClient
            .Setup(x => x.IsHealthyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        ActionResult<object> result = await this.controller.GetAIHealth(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? isHealthyProperty = responseType.GetProperty("isHealthy");
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");

        Assert.False((bool)isHealthyProperty!.GetValue(response) !);
        Assert.Equal("Unavailable", statusProperty!.GetValue(response));
    }

    [Fact]
    public Task GetAIHealth_WhenServiceThrowsException_ThrowsException()
    {
        // Arrange
        this.mockAIAssistantClient
            .Setup(x => x.IsHealthyAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Health check timed out"));

        // Act & Assert
        return Assert.ThrowsAsync<TimeoutException>(
            () => this.controller.GetAIHealth(CancellationToken.None));
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetMotivation_LogsAthleteNameCorrectly()
    {
        // Arrange
        AIMotivationRequestDto request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Logged Athlete",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Competition",
            },
        };

        AIMotivationResponseDto response = new AIMotivationResponseDto
        {
            MotivationalMessage = "Test message",
            Source = "Test-Source",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetMotivationAsync(It.IsAny<AIMotivationRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        // Verify that logging was called with correct athlete name
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() !.Contains("Logged Athlete")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify response source was logged
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() !.Contains("Test-Source")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkoutAnalysis_LogsWorkoutCountCorrectly()
    {
        // Arrange
        AIWorkoutAnalysisRequestDto request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto { ActivityType = "Run" },
                new AIWorkoutDataDto { ActivityType = "Bike" },
                new AIWorkoutDataDto { ActivityType = "Swim" },
            },
            AnalysisType = "TestAnalysis",
        };

        AIWorkoutAnalysisResponseDto response = new AIWorkoutAnalysisResponseDto
        {
            Analysis = "Test analysis",
            Source = "Test-Source",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockAIAssistantClient
            .Setup(x => x.GetWorkoutAnalysisAsync(It.IsAny<AIWorkoutAnalysisRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await this.controller.GetWorkoutAnalysis(request, CancellationToken.None);

        // Assert
        // Verify workout count and analysis type were logged
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() !.Contains("3") && v.ToString() !.Contains("TestAnalysis")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}