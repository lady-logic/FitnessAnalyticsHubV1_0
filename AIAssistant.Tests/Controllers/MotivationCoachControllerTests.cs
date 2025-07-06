using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.UI.API.Controllers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AIAssistant.Tests.Controllers;

public class MotivationCoachControllerTests
{
    private readonly Mock<IMotivationCoachService> _mockMotivationService;
    private readonly Mock<ILogger<MotivationCoachController>> _mockLogger;
    private readonly MotivationCoachController _controller;

    public MotivationCoachControllerTests()
    {
        _mockMotivationService = new Mock<IMotivationCoachService>();
        _mockLogger = new Mock<ILogger<MotivationCoachController>>();
        _controller = new MotivationCoachController(_mockMotivationService.Object, _mockLogger.Object);
    }

    #region GetMotivation Tests

    [Fact]
    public async Task GetMotivation_WithValidRequest_ReturnsOkWithMotivationResponse()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Id = "1",
                Name = "Test Athlete",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss"
            },
            IsStruggling = false,
            UpcomingWorkoutType = "Running"
        };

        var expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "You're doing great! Keep pushing towards your weight loss goals.",
            Quote = "Success is the sum of small efforts repeated day in and day out.",
            ActionableTips = new List<string>
            {
                "Focus on consistency over perfection",
                "Celebrate small victories",
                "Stay hydrated during your runs"
            },
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetMotivation(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Equal(expectedResponse.MotivationalMessage, response.MotivationalMessage);
        Assert.Equal(expectedResponse.Quote, response.Quote);
        Assert.Equal(expectedResponse.ActionableTips.Count, response.ActionableTips?.Count);

        // Verify service was called
        _mockMotivationService.Verify(
            s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivation_WithStrugglingAthlete_ReturnsMotivationResponse()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Struggling Athlete",
                FitnessLevel = "Beginner",
                PrimaryGoal = "General Fitness"
            },
            IsStruggling = true
        };

        var expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "It's okay to struggle - that's how we grow stronger! Every step forward counts.",
            Quote = "The strongest people are not those who show strength in front of us, but those who win battles we know nothing about.",
            ActionableTips = new List<string>
            {
                "Start with just 10 minutes of activity",
                "Remember why you started",
                "Ask for support when you need it"
            },
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetMotivation(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Contains("struggle", response.MotivationalMessage, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(response.ActionableTips);
        Assert.True(response.ActionableTips.Count > 0);

        // Verify the request was passed correctly
        _mockMotivationService.Verify(
            s => s.GetHuggingFaceMotivationalMessageAsync(
                It.Is<MotivationRequestDto>(r => r.IsStruggling == true)),
            Times.Once);
    }

    [Fact]
    public async Task GetMotivation_WhenServiceThrowsException_Returns500()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Test Athlete",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance"
            }
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _controller.GetMotivation(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Contains("error occurred", statusCodeResult.Value?.ToString());
    }

    #endregion

    #region GetHuggingFaceMotivation Tests

    [Fact]
    public async Task GetHuggingFaceMotivation_WithValidRequest_ReturnsOkWithMotivationResponse()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "HuggingFace Test",
                FitnessLevel = "Expert",
                PrimaryGoal = "Competition"
            }
        };

        var expectedResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Champion mindset! You're training for excellence.",
            Quote = "Champions are made in the gym, legends are made through dedication.",
            ActionableTips = new List<string>
            {
                "Visualize your competition success",
                "Focus on technique perfection",
                "Trust your training process"
            },
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetHuggingFaceMotivation(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MotivationResponseDto>(okResult.Value);

        Assert.Contains("Champion", response.MotivationalMessage);
        Assert.Contains("competition", response.ActionableTips[0], StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_WhenServiceIsHealthy_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        var healthyResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Health check passed!",
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(healthyResponse);

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        // Use reflection to check anonymous object properties
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        var messageProperty = responseType.GetProperty("message");
        var timestampProperty = responseType.GetProperty("timestamp");

        Assert.Equal("healthy", statusProperty?.GetValue(response));
        Assert.Equal("HuggingFace service is responding", messageProperty?.GetValue(response));
        Assert.NotNull(timestampProperty?.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_WhenServiceThrowsException_Returns503()
    {
        // Arrange
        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ThrowsAsync(new Exception("Service down"));

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);

        var response = statusCodeResult.Value;
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        var messageProperty = responseType.GetProperty("message");

        Assert.Equal("unhealthy", statusProperty?.GetValue(response));
        Assert.Contains("Service down", messageProperty?.GetValue(response)?.ToString());
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetMotivation_LogsAthleteNameCorrectly()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Logged Athlete",
                FitnessLevel = "Intermediate"
            }
        };

        var response = new MotivationResponseDto
        {
            MotivationalMessage = "Test message",
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(response);

        // Act
        await _controller.GetMotivation(request);

        // Assert
        _mockLogger.Verify(
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
    public async Task GetMotivation_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = null!, // Null profile
            IsStruggling = false
        };

        var fallbackResponse = new MotivationResponseDto
        {
            MotivationalMessage = "Keep going! You've got this!",
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(fallbackResponse);

        // Act
        var result = await _controller.GetMotivation(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetMotivation_WithInvalidAthleteName_StillReturnsMotivation(string? invalidName)
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = invalidName!,
                FitnessLevel = "Beginner"
            }
        };

        var response = new MotivationResponseDto
        {
            MotivationalMessage = "Every journey starts with a single step!",
            GeneratedAt = DateTime.UtcNow
        };

        _mockMotivationService
            .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetMotivation(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var motivationResponse = Assert.IsType<MotivationResponseDto>(okResult.Value);
        Assert.NotEmpty(motivationResponse.MotivationalMessage);
    }

    #endregion
}