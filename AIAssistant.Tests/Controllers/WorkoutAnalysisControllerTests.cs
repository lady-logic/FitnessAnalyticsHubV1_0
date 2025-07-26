namespace AIAssistant.Tests.Controllers;

using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Tests.Base;
using AIAssistant.Tests.Helpers;
using AIAssistant.UI.API.Controllers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class WorkoutAnalysisControllerTests : AIAssistantControllerTestBase<WorkoutAnalysisController>
{
    private readonly Mock<IWorkoutAnalysisService> mockWorkoutAnalysisService;
    private readonly Mock<ILogger<WorkoutAnalysisController>> mockLogger;
    private readonly WorkoutAnalysisController controller;

    public WorkoutAnalysisControllerTests()
    {
        this.mockWorkoutAnalysisService = MockSetup.CreateMockWorkoutAnalysisService();
        this.mockLogger = MockSetup.CreateMockLogger<WorkoutAnalysisController>();
        this.controller = new WorkoutAnalysisController(this.mockWorkoutAnalysisService.Object, this.mockLogger.Object, CancellationToken.None);
    }

    #region AnalyzeHuggingFaceWorkouts Tests

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_WithValidRequest_ReturnsAnalysisResult()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);
        Assert.Equal(expectedResponse.Provider, response.Provider);
        Assert.NotEmpty(response.KeyInsights!);
        Assert.NotEmpty(response.Recommendations!);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("HuggingFace service unavailable"));

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing workouts", statusCodeResult.Value);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkouts_LogsAnalysisType()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = MockSetup.CreateTestWorkoutAnalysisRequest("Performance");
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);

        // Assert
        MockSetup.VerifyLoggerCalledWithInformation(this.mockLogger, "Performance", Times.Once());
    }

    #endregion

    #region AnalyzeGoogleGeminiWorkouts Tests

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkouts_WithValidRequest_ReturnsAnalysisResult()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("GoogleGemini");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeGoogleGeminiWorkouts(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);
        Assert.Equal("GoogleGemini", response.Provider);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkouts_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("GoogleGemini service unavailable"));

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeGoogleGeminiWorkouts(request, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing workouts", statusCodeResult.Value);
    }

    #endregion

    #region AnalyzePerformanceTrends Tests

    [Fact]
    public async Task AnalyzePerformanceTrends_WithValidAthleteId_ReturnsAnalysisResult()
    {
        // Arrange
        int athleteId = 123;
        string timeFrame = "month";
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzePerformanceTrends(athleteId, CancellationToken.None, timeFrame);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Trends" &&
                req.AdditionalContext!.ContainsKey("athleteId") &&
                req.AdditionalContext["athleteId"].Equals(athleteId)), CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("year")]
    public async Task AnalyzePerformanceTrends_WithDifferentTimeFrames_PassesCorrectTimeFrame(string timeFrame)
    {
        // Arrange
        int athleteId = 123;
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzePerformanceTrends(athleteId, CancellationToken.None, timeFrame);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AdditionalContext!.ContainsKey("timeFrame") &&
                req.AdditionalContext["timeFrame"].Equals(timeFrame)), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzePerformanceTrends_WithDefaultTimeFrame_UsesMonth()
    {
        // Arrange
        int athleteId = 123;
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act - Not providing timeFrame parameter should default to "month"
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzePerformanceTrends(athleteId, CancellationToken.None);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AdditionalContext!.ContainsKey("timeFrame") &&
                req.AdditionalContext["timeFrame"].Equals("month")), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzePerformanceTrends_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int athleteId = 123;

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Performance trends service error"));

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzePerformanceTrends(athleteId, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing performance trends", statusCodeResult.Value);
    }

    #endregion

    #region GetTrainingRecommendations Tests

    [Fact]
    public async Task GetTrainingRecommendations_WithValidAthleteId_ReturnsRecommendations()
    {
        // Arrange
        int athleteId = 456;
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.GetTrainingRecommendations(athleteId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Recommendations" &&
                req.AdditionalContext!.ContainsKey("focus") &&
                req.AdditionalContext["focus"].Equals("training_optimization")), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task GetTrainingRecommendations_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int athleteId = 456;

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Training recommendations service error"));

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.GetTrainingRecommendations(athleteId, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while getting training recommendations", statusCodeResult.Value);
    }

    #endregion

    #region AnalyzeHealthMetrics Tests

    [Fact]
    public async Task AnalyzeHealthMetrics_WithValidRequest_ReturnsHealthAnalysis()
    {
        // Arrange
        HealthAnalysisRequestDto request = CreateValidHealthAnalysisRequest();
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHealthMetrics(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Health" &&
                req.AdditionalContext!.ContainsKey("focus") &&
                req.AdditionalContext["focus"].Equals("injury_prevention")), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeHealthMetrics_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        HealthAnalysisRequestDto request = CreateValidHealthAnalysisRequest();

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Health metrics service error"));

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHealthMetrics(request, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while analyzing health metrics", statusCodeResult.Value);
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_WhenServiceIsHealthy_ReturnsHealthyStatus()
    {
        // Arrange
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");
        expectedResponse.Analysis = "Health check successful";

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult result = await this.controller.HealthCheck(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");
        System.Reflection.PropertyInfo? analysisGeneratedProperty = responseType.GetProperty("analysisGenerated");

        Assert.Equal("healthy", statusProperty!.GetValue(response));
        Assert.Equal("Workout analysis service is responding", messageProperty!.GetValue(response));
        Assert.True((bool)analysisGeneratedProperty!.GetValue(response)!);
    }

    [Fact]
    public async Task HealthCheck_WhenServiceThrowsException_ReturnsUnhealthyStatus()
    {
        // Arrange
        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Service is down"));

        // Act
        ActionResult result = await this.controller.HealthCheck(CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);

        object? response = statusCodeResult.Value;
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");

        Assert.Equal("unhealthy", statusProperty!.GetValue(response));
        Assert.Equal("Service is down", messageProperty!.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_CreatesValidTestRequest()
    {
        // Arrange
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        await this.controller.HealthCheck(CancellationToken.None);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == "Health Check" &&
                req.RecentWorkouts.Count == 1 &&
                req.RecentWorkouts[0].ActivityType == "Run" &&
                req.AthleteProfile!.Name == "Test User"), CancellationToken.None),
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
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();
        request.AnalysisType = analysisType;
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        WorkoutAnalysisResponseDto response = Assert.IsType<WorkoutAnalysisResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.Analysis, response.Analysis);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.AnalysisType == analysisType), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task MultipleEndpoints_WithSameService_CallsServiceSeparately()
    {
        // Arrange
        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();
        int athleteId = 123;
        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(CreateMockWorkoutAnalysisResponse("GoogleGemini"));

        // Act
        await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);
        await this.controller.AnalyzeGoogleGeminiWorkouts(request, CancellationToken.None);
        await this.controller.GetTrainingRecommendations(athleteId, CancellationToken.None);

        // Assert
        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
            Times.Exactly(2)); // Once for direct call, once for training recommendations

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task AllEndpoints_WhenServiceReturnsNull_HandleGracefully()
    {
        // Arrange
        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync((WorkoutAnalysisResponseDto?)null);

        WorkoutAnalysisRequestDto request = CreateValidWorkoutAnalysisRequest();

        // Act & Assert - Should not throw null reference exceptions
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHuggingFaceWorkouts(request, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task AnalyzeHealthMetrics_WithEmptyWorkoutList_StillCallsService()
    {
        // Arrange
        HealthAnalysisRequestDto request = new HealthAnalysisRequestDto
        {
            AthleteId = 123,
            RecentWorkouts = new List<WorkoutDataDto>(), // Empty list
        };

        WorkoutAnalysisResponseDto expectedResponse = CreateMockWorkoutAnalysisResponse("HuggingFace");

        this.mockWorkoutAnalysisService
            .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<WorkoutAnalysisResponseDto> result = await this.controller.AnalyzeHealthMetrics(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);

        this.mockWorkoutAnalysisService.Verify(
            s => s.AnalyzeHuggingFaceWorkoutsAsync(
                It.Is<WorkoutAnalysisRequestDto>(req =>
                req.RecentWorkouts.Count == 0), CancellationToken.None),
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