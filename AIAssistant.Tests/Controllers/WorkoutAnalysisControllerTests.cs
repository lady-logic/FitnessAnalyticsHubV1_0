using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Tests.Base;
using AIAssistant.Tests.Helpers;
using AIAssistant.UI.API.Controllers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AIAssistant.Tests.Controllers;

public class WorkoutAnalysisControllerTests : AIAssistantControllerTestBase<WorkoutAnalysisController>
{
    private readonly Mock<IWorkoutAnalysisService> mockWorkoutAnalysisService;
    private readonly Mock<ILogger<WorkoutAnalysisController>> mockLogger;
    private readonly WorkoutAnalysisController controller;

    public WorkoutAnalysisControllerTests()
    {
        this.mockWorkoutAnalysisService = MockSetup.CreateMockWorkoutAnalysisService();
        this.mockLogger = MockSetup.CreateMockLogger<WorkoutAnalysisController>();
        this.controller = new WorkoutAnalysisController(this.mockWorkoutAnalysisService.Object, this.mockLogger.Object);
    }

    #region AnalyzeHuggingFaceWorkouts Tests

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_WithValidRequest_ReturnsAnalysisResult()
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzeHuggingFaceWorkouts(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);
        Assert.Equal(expectedResponse.Provider, response.Provider);
        Assert.NotEmpty(response.KeyInsights!);
        Assert.NotEmpty(response.Recommendations!);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("HuggingFace service unavailable"));

        // Act
        var result = await this.controller.AnalyzeHuggingFaceWorkouts(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing workouts", statusCodeResult.Value);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_LogsAnalysisType()
    {
        // Arrange
        var request = MockSetup.CreateTestWorkoutAnalysisRequest("Performance");
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await this.controller.AnalyzeHuggingFaceWorkouts(request);

        // Assert
        MockSetup.VerifyLoggerCalledWithInformation(this.mockLogger, "Performance", Times.Once());
    }

    #endregion

    #region AnalyzeGoogleGeminiWorkouts Tests

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkouts_WithValidRequest_ReturnsAnalysisResult()
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();
        var expectedResponse = CreateMockWorkoutAnalysisResponse("GoogleGemini");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzeGoogleGeminiWorkouts(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);
        Assert.Equal("GoogleGemini", response.Provider);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkouts_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("GoogleGemini service unavailable"));

        // Act
        var result = await this.controller.AnalyzeGoogleGeminiWorkouts(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing workouts", statusCodeResult.Value);
    }

    #endregion

    #region AnalyzePerformanceTrends Tests

    [Fact]
    public async Task AnalyzePerformanceTrends_WithValidAthleteId_ReturnsAnalysisResult()
    {
        // Arrange
        var athleteId = 123;
        var timeFrame = "month";
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzePerformanceTrends(athleteId, timeFrame);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Trends" &&
                req.AdditionalContext!.ContainsKey("athleteId") &&
                req.AdditionalContext["athleteId"].Equals(athleteId))),
            Times.Once);
    }

    [Theory]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("year")]
    public async Task AnalyzePerformanceTrends_WithDifferentTimeFrames_PassesCorrectTimeFrame(string timeFrame)
    {
        // Arrange
        var athleteId = 123;
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzePerformanceTrends(athleteId, timeFrame);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AdditionalContext!.ContainsKey("timeFrame") &&
                req.AdditionalContext["timeFrame"].Equals(timeFrame))),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzePerformanceTrends_WithDefaultTimeFrame_UsesMonth()
    {
        // Arrange
        var athleteId = 123;
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act - Not providing timeFrame parameter should default to "month"
        var result = await this.controller.AnalyzePerformanceTrends(athleteId);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AdditionalContext!.ContainsKey("timeFrame") &&
                req.AdditionalContext["timeFrame"].Equals("month"))),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzePerformanceTrends_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var athleteId = 123;

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("Performance trends service error"));

        // Act
        var result = await this.controller.AnalyzePerformanceTrends(athleteId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing performance trends", statusCodeResult.Value);
    }

    #endregion

    #region GetTrainingRecommendations Tests

    [Fact]
    public async Task GetTrainingRecommendations_WithValidAthleteId_ReturnsRecommendations()
    {
        // Arrange
        var athleteId = 456;
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.GetTrainingRecommendations(athleteId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Recommendations" &&
                req.AdditionalContext!.ContainsKey("focus") &&
                req.AdditionalContext["focus"].Equals("training_optimization"))),
            Times.Once);
    }

    [Fact]
    public async Task GetTrainingRecommendations_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var athleteId = 456;

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("Training recommendations service error"));

        // Act
        var result = await this.controller.GetTrainingRecommendations(athleteId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while getting training recommendations", statusCodeResult.Value);
    }

    #endregion

    #region AnalyzeHealthMetrics Tests

    [Fact]
    public async Task AnalyzeHealthMetrics_WithValidRequest_ReturnsHealthAnalysis()
    {
        // Arrange
        var request = CreateValidHealthAnalysisRequest();
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzeHealthMetrics(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Health" &&
                req.AdditionalContext!.ContainsKey("focus") &&
                req.AdditionalContext["focus"].Equals("injury_prevention"))),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeHealthMetrics_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = CreateValidHealthAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("Health metrics service error"));

        // Act
        var result = await this.controller.AnalyzeHealthMetrics(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing health metrics", statusCodeResult.Value);
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_WhenServiceIsHealthy_ReturnsHealthyStatus()
    {
        // Arrange
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");
        expectedResponse.Analysis = "Health check successful";

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.HealthCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        var messageProperty = responseType.GetProperty("message");
        var analysisGeneratedProperty = responseType.GetProperty("analysisGenerated");

        Assert.Equal("healthy", statusProperty!.GetValue(response));
        Assert.Equal("Workout analysis service is responding", messageProperty!.GetValue(response));
        Assert.True((bool)analysisGeneratedProperty!.GetValue(response)!);
    }

    [Fact]
    public async Task HealthCheck_WhenServiceThrowsException_ReturnsUnhealthyStatus()
    {
        // Arrange
        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("Service is down"));

        // Act
        var result = await this.controller.HealthCheck();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);

        var response = statusCodeResult.Value;
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        var messageProperty = responseType.GetProperty("message");

        Assert.Equal("unhealthy", statusProperty!.GetValue(response));
        Assert.Equal("Service is down", messageProperty!.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_CreatesValidTestRequest()
    {
        // Arrange
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await this.controller.HealthCheck();

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Health Check" &&
                req.RecentWorkouts.Count == 1 &&
                req.RecentWorkouts[0].ActivityType == "Run" &&
                req.AthleteProfile!.Name == "Test User")),
            Times.Once);
    }

    #endregion

    #region Integration Tests

    [Theory]
    [InlineData("Performance")]
    [InlineData("Health")]
    [InlineData("Trends")]
    [InlineData("Custom")]
    public async Task AnalyzeHuggingFaceWorkouts_WithDifferentAnalysisTypes_HandlesCorrectly(string analysisType)
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();
        request.AnalysisType = analysisType;
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzeHuggingFaceWorkouts(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == analysisType)),
            Times.Once);
    }

    [Fact]
    public async Task MultipleEndpoints_WithSameService_CallsServiceSeparately()
    {
        // Arrange
        var request = CreateValidWorkoutAnalysisRequest();
        var athleteId = 123;
        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(CreateMockWorkoutAnalysisResponse("GoogleGemini"));

        // Act
        await this.controller.AnalyzeHuggingFaceWorkouts(request);
        await this.controller.AnalyzeGoogleGeminiWorkouts(request);
        await this.controller.GetTrainingRecommendations(athleteId);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()),
            Times.Exactly(2)); // Once for direct call, once for training recommendations

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()),
            Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task AllEndpoints_WhenServiceReturnsNull_HandleGracefully()
    {
        // Arrange
        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync((WorkoutAnalysisResponseDto?)null);

        var request = CreateValidWorkoutAnalysisRequest();

        // Act & Assert - Should not throw null reference exceptions
        var result = await this.controller.AnalyzeHuggingFaceWorkouts(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task AnalyzeHealthMetrics_WithEmptyWorkoutList_StillCallsService()
    {
        // Arrange
        var request = new HealthAnalysisRequestDto
        {
            AthleteId = 123,
            RecentWorkouts = new List<WorkoutDataDto>(), // Empty list
        };

        var expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await this.controller.AnalyzeHealthMetrics(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.Is<WorkoutAnalysisRequestDto>(req =>
                req.RecentWorkouts.Count == 0)),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static WorkoutAnalysisRequestDto CreateValidWorkoutAnalysisRequest()
    {
        return MockSetup.CreateTestWorkoutAnalysisRequest();
    }

    private static HealthAnalysisRequestDto CreateValidHealthAnalysisRequest()
    {
        return MockSetup.CreateTestHealthAnalysisRequest();
    }

    private static WorkoutAnalysisResponseDto CreateMockWorkoutAnalysisResponse(string provider)
    {
        return new WorkoutAnalysisResponseDto
        {
            Analysis = $"Test analysis from {provider}",
            KeyInsights = new List<string>
            {
                "Insight 1",
                "Insight 2",
                "Insight 3",
            },
            Recommendations = new List<string>
            {
                "Recommendation 1",
                "Recommendation 2",
            },
            GeneratedAt = DateTime.UtcNow,
            Provider = provider,
            RequestId = Guid.NewGuid().ToString(),
        };
    }

    #endregion
}