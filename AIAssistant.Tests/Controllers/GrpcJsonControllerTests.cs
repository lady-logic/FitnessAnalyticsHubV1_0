namespace AIAssistant.Tests.Controllers;

using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.Tests.Base;
using AIAssistant.UI.API.Controllers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

public class GrpcJsonControllerTests : AIAssistantControllerTestBase<GrpcJsonController>
{
    private readonly Mock<IMotivationCoachService> mockMotivationService;
    private readonly Mock<IWorkoutAnalysisService> mockWorkoutAnalysisService;
    private readonly Mock<ILogger<GrpcJsonController>> mockLogger;
    private readonly GrpcJsonController controller;

    public GrpcJsonControllerTests()
    {
        this.mockMotivationService = new Mock<IMotivationCoachService>();
        this.mockWorkoutAnalysisService = new Mock<IWorkoutAnalysisService>();
        this.mockLogger = this.CreateMockLogger<GrpcJsonController>();

        this.controller = new GrpcJsonController(
            this.mockMotivationService.Object,
            this.mockWorkoutAnalysisService.Object,
            this.mockLogger.Object);
    }

    private GrpcJsonMotivationRequestDto CreateMotivationRequest()
    {
        return new GrpcJsonMotivationRequestDto
        {
            AthleteProfile = new GrpcJsonAthleteProfileDto
            {
                Name = "John Doe",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss",
            },
        };
    }

    private GrpcJsonWorkoutAnalysisRequestDto CreateWorkoutAnalysisRequest()
    {
        return new GrpcJsonWorkoutAnalysisRequestDto
        {
            AnalysisType = "Performance",
            AthleteProfile = new GrpcJsonAthleteProfileDto
            {
                Name = "Jane Smith",
                FitnessLevel = "Advanced",
            },
            RecentWorkouts = new[]
            {
                new GrpcJsonWorkoutDto
                {
                    Date = "2025-01-15",
                    ActivityType = "Run",
                    Distance = 5000,
                    Duration = 1800,
                    Calories = 300,
                },
            },
        };
    }

    [Fact]
    public async Task GetMotivation_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = this.CreateMotivationRequest();
        var serviceResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Keep going!",
            Quote = "Success is a journey",
            ActionableTips = new List<string> { "Stay hydrated", "Get enough sleep" },
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await this.controller.GetMotivationAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify service was called
        this.mockMotivationService.Verify(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutAnalysis_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = this.CreateWorkoutAnalysisRequest();
        var serviceResponse = new WorkoutAnalysisResponseDto
        {
            Analysis = "Great progress in your training",
            KeyInsights = new List<string> { "Consistent pace", "Good endurance" },
            Recommendations = new List<string> { "Increase distance", "Add intervals" },
            GeneratedAt = DateTime.UtcNow,
            Provider = "GoogleGemini",
        };

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await this.controller.GetWorkoutAnalysisAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify service was called
        this.mockWorkoutAnalysisService.Verify(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkouts_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = this.CreateWorkoutAnalysisRequest();
        var serviceResponse = new WorkoutAnalysisResponseDto
        {
            Analysis = "GoogleGemini analysis result",
            KeyInsights = new List<string> { "AI insight" },
            Recommendations = new List<string> { "AI recommendation" },
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await this.controller.AnalyzeGoogleGeminiWorkoutsAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var result = await this.controller.HealthCheckAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Check if response contains expected structure
        var response = okResult.Value;
        var json = JsonConvert.SerializeObject(response);
        Assert.Contains("healthy", json);
        Assert.Contains("gRPC-JSON Bridge", json);
    }

    [Fact]
    public async Task GetPerformanceTrends_WithValidRequest_ReturnsMockData()
    {
        // Arrange
        var request = new GrpcJsonPerformanceTrendsRequestDto
        {
            AthleteId = 123,
            TimeFrame = "30days",
        };

        // Act
        var result = await this.controller.GetPerformanceTrendsAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var json = JsonConvert.SerializeObject(okResult.Value);
        Assert.Contains("Performance trends analysis", json);
        Assert.Contains("123", json);
    }

    [Fact]
    public async Task GetTrainingRecommendations_WithValidRequest_ReturnsMockData()
    {
        // Arrange
        var request = new GrpcJsonTrainingRecommendationsRequestDto
        {
            AthleteId = 456,
        };

        // Act
        var result = await this.controller.GetTrainingRecommendationsAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var json = JsonConvert.SerializeObject(okResult.Value);
        Assert.Contains("Training recommendations", json);
        Assert.Contains("456", json);
    }

    [Fact]
    public async Task AnalyzeHealthMetrics_WithValidRequest_ReturnsMockData()
    {
        // Arrange
        var request = new GrpcJsonHealthMetricsRequestDto
        {
            AthleteId = 789,
            RecentWorkouts = new[]
            {
                new GrpcJsonWorkoutDto { Calories = 300 },
                new GrpcJsonWorkoutDto { Calories = 250 },
            },
        };

        // Act
        var result = await this.controller.AnalyzeHealthMetricsAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var json = JsonConvert.SerializeObject(okResult.Value);
        Assert.Contains("Health metrics analysis", json);
        Assert.Contains("789", json);
        Assert.Contains("275", json); // Average calories
    }

    [Fact]
    public async Task GetMotivation_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        var request = new GrpcJsonMotivationRequestDto
        {
            AthleteProfile = null,
        };

        var serviceResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Default motivation",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await this.controller.GetMotivationAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetWorkoutAnalysis_WithEmptyWorkouts_HandlesGracefully()
    {
        // Arrange
        var request = new GrpcJsonWorkoutAnalysisRequestDto
        {
            AnalysisType = "Performance",
            RecentWorkouts = null,
        };

        var serviceResponse = new WorkoutAnalysisResponseDto
        {
            Analysis = "No workouts to analyze",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await this.controller.GetWorkoutAnalysisAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}