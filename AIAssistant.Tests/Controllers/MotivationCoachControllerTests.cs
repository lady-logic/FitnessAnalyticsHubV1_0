namespace AIAssistant.Tests.Controllers;

using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.UI.API.Controllers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class MotivationCoachControllerTests
{
    private readonly Mock<IMotivationCoachService> mockMotivationService;
    private readonly Mock<ILogger<MotivationCoachController>> mockLogger;
    private readonly MotivationCoachController controller;

    public MotivationCoachControllerTests()
    {
        this.mockMotivationService = new Mock<IMotivationCoachService>();
        this.mockLogger = new Mock<ILogger<MotivationCoachController>>();
        this.controller = new MotivationCoachController(this.mockMotivationService.Object, this.mockLogger.Object);
    }

    #region GetMotivation Tests

    [Fact]
    public async Task GetMotivation_WithValidRequest_ReturnsOkWithMotivationResponse()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Id = "1",
                Name = "Test Athlete",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss",
            },
            IsStruggling = false,
            UpcomingWorkoutType = "Running",
        };

        MotivationResponseDto expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "You're doing great! Keep pushing towards your weight loss goals.",
            Quote = "Success is the sum of small efforts repeated day in and day out.",
            ActionableTips = new List<string>
            {
                "Focus on consistency over perfection",
                "Celebrate small victories",
                "Stay hydrated during your runs",
            },
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        MotivationResponseDto response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.MotivationalMessage, response.MotivationalMessage);
        Assert.Equal(expectedResponse.Quote, response.Quote);
        Assert.Equal(expectedResponse.ActionableTips.Count, response.ActionableTips?.Count);

        // Verify service was called
        this.mockMotivationService.Verify(
            s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivation_WithStrugglingAthlete_ReturnsMotivationResponse()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Struggling Athlete",
                FitnessLevel = "Beginner",
                PrimaryGoal = "General Fitness",
            },
            IsStruggling = true,
        };

        MotivationResponseDto expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "It's okay to struggle - that's how we grow stronger! Every step forward counts.",
            Quote = "The strongest people are not those who show strength in front of us, but those who win battles we know nothing about.",
            ActionableTips = new List<string>
            {
                "Start with just 10 minutes of activity",
                "Remember why you started",
                "Ask for support when you need it",
            },
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        MotivationResponseDto response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Contains("struggle", response.MotivationalMessage, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(response.ActionableTips);
        Assert.True(response.ActionableTips.Count > 0);

        // Verify the request was passed correctly
        this.mockMotivationService.Verify(
            s => s.GetHuggingFaceMotivationalMessageAsync(
                It.Is<MotivationRequestDto>(r => r.IsStruggling == true), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivation_WhenServiceThrowsException_Returns500()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Test Athlete",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance",
            },
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Contains("error occurred", statusCodeResult.Value?.ToString());
    }

    #endregion

    #region GetHuggingFaceMotivation Tests

    [Fact]
    public async Task GetHuggingFaceMotivation_WithValidRequest_ReturnsOkWithMotivationResponse()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "HuggingFace Test",
                FitnessLevel = "Expert",
                PrimaryGoal = "Competition",
            },
        };

        MotivationResponseDto expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Champion mindset! You're training for excellence.",
            Quote = "Champions are made in the gym, legends are made through dedication.",
            ActionableTips = new List<string>
            {
                "Visualize your competition success",
                "Focus on technique perfection",
                "Trust your training process",
            },
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetHuggingFaceMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        MotivationResponseDto response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Contains("Champion", response.MotivationalMessage);
        Assert.Contains("competition", response.ActionableTips[0], StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_WhenServiceIsHealthy_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        MotivationResponseDto healthyResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Health check passed!",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(healthyResponse);

        // Act
        ActionResult result = await this.controller.HealthCheck(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        // Use reflection to check anonymous object properties
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");
        System.Reflection.PropertyInfo? timestampProperty = responseType.GetProperty("timestamp");

        Assert.Equal("healthy", statusProperty?.GetValue(response));
        Assert.Equal("HuggingFace service is responding", messageProperty?.GetValue(response));
        Assert.NotNull(timestampProperty?.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_WhenServiceThrowsException_Returns503()
    {
        // Arrange
        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Service down"));

        // Act
        ActionResult result = await this.controller.HealthCheck(CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);

        object? response = statusCodeResult.Value;
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");

        Assert.Equal("unhealthy", statusProperty?.GetValue(response));
        Assert.Contains("Service down", messageProperty?.GetValue(response)?.ToString());
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetMotivation_LogsAthleteNameCorrectly()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Logged Athlete",
                FitnessLevel = "Intermediate",
            },
        };

        MotivationResponseDto response = new MotivationResponseDto
        {
            MotivationalMessage = "Test message",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logged Athlete")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Input Validation Tests

    [Fact]
    public async Task GetMotivation_WithNullAthleteProfile_ReturnsInternalServerError()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = null!,
            IsStruggling = false,
        };

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Contains("error occurred", objectResult.Value?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetMotivation_WithInvalidAthleteName_StillReturnsMotivation(string? invalidName)
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = invalidName!,
                FitnessLevel = "Beginner",
            },
        };

        MotivationResponseDto response = new MotivationResponseDto
        {
            MotivationalMessage = "Every journey starts with a single step!",
            GeneratedAt = DateTime.UtcNow,
        };

        this.mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        ActionResult<MotivationResponseDto> result = await this.controller.GetMotivation(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        MotivationResponseDto motivationResponse = Assert.IsType<MotivationResponseDto>(okResult.Value);
        Assert.NotEmpty(motivationResponse.MotivationalMessage);
    }

    #endregion
}