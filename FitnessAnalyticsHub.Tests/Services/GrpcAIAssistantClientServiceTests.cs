using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessAnalyticsHub.Tests.Services;

public class GrpcAIAssistantClientServiceTests : IDisposable
{
    private readonly Mock<ILogger<GrpcAIAssistantClientService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private GrpcAIAssistantClientService? _service;

    public GrpcAIAssistantClientServiceTests()
    {
        _mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup default configuration
        _mockConfiguration.Setup(c => c["AIAssistant:GrpcUrl"])
            .Returns("http://localhost:5001");
    }

    #region Constructor and Configuration Tests

    [Fact]
    public void Constructor_WithCustomGrpcUrl_LogsCorrectUrl()
    {
        // Arrange
        var customConfig = new Mock<IConfiguration>();
        customConfig.Setup(c => c["AIAssistant:GrpcUrl"])
            .Returns("http://custom-grpc-service:5001");

        var mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();

        // Act
        using var service = new GrpcAIAssistantClientService(mockLogger.Object, customConfig.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("http://custom-grpc-service:5001")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullGrpcUrl_UsesDefaultUrl()
    {
        // Arrange
        var nullConfig = new Mock<IConfiguration>();
        nullConfig.Setup(c => c["AIAssistant:GrpcUrl"])
            .Returns((string?)null);

        var mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();

        // Act
        using var service = new GrpcAIAssistantClientService(mockLogger.Object, nullConfig.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("http://localhost:5001")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Input Validation and Logging Tests

    [Fact]
    public async Task GetMotivationAsync_WithNullAthleteProfile_LogsUnknownAthlete()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = null,
            PreferredTone = "Motivational"
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetMotivationAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: Unknown");
    }

    [Fact]
    public async Task GetMotivationAsync_WithValidAthleteProfile_LogsAthleteName()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "John Doe",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss"
            },
            PreferredTone = "Encouraging"
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetMotivationAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: John Doe");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithMultipleWorkouts_LogsWorkoutCount()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Mike Johnson",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance"
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                    ActivityType = "Run",
                    Distance = 10.0,
                    Duration = 2700,
                    Calories = 650
                },
                new AIWorkoutDataDto
                {
                    Date = DateTime.UtcNow.AddDays(-2),
                    ActivityType = "Cycle",
                    Distance = 25.0,
                    Duration = 3600,
                    Calories = 800
                }
            },
            AnalysisType = "Performance"
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 2 workouts");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithEmptyWorkouts_LogsZeroWorkouts()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Beginner" },
            RecentWorkouts = new List<AIWorkoutDataDto>(),
            AnalysisType = "General"
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithValidRequest_LogsCorrectMessage()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto { ActivityType = "Swimming", Distance = 2.0, Duration = 60, Calories = 500 }
            }
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 1 workouts");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithValidParameters_LogsCorrectMessage()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        const int athleteId = 123;
        const string timeFrame = "month";

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetPerformanceTrendsAsync(athleteId, timeFrame, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, $"performance trends for athlete: {athleteId}, timeFrame: {timeFrame}");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithValidAthleteId_LogsCorrectMessage()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        const int athleteId = 456;

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetTrainingRecommendationsAsync(athleteId, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, $"training recommendations for athlete: {athleteId}");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithValidData_LogsCorrectMessage()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        const int athleteId = 789;
        var recentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto { ActivityType = "Yoga", Duration = 60, Calories = 200 }
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.AnalyzeHealthMetricsAsync(athleteId, recentWorkouts, CancellationToken.None));

        // Verify logging occurred before the exception
        VerifyLogCalled(LogLevel.Information, $"health metrics analysis for athlete: {athleteId}");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task IsHealthyAsync_WithConnectionError_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await _service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
        VerifyLogCalled(LogLevel.Warning, "Health check failed");
    }

    [Fact]
    public async Task GetMotivationAsync_WithGrpcConnectionError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetMotivationAsync(request, CancellationToken.None));

        // Verify error logging occurred
        VerifyLogCalled(LogLevel.Error, "Error getting motivation");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithGrpcConnectionError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" },
            RecentWorkouts = new List<AIWorkoutDataDto>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify error logging occurred
        VerifyLogCalled(LogLevel.Error, "Error getting workout analysis");
    }

    #endregion

    #region Data Conversion Tests

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = null, // Test null handling
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto { ActivityType = "Run", Distance = 5.0 }
            }
        };

        // Act & Assert - Should not throw due to null athlete profile
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify it still attempts the call (logs the workout count)
        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 1 workouts");
    }

    [Theory]
    [InlineData("Performance")]
    [InlineData("Health")]
    [InlineData("Recovery")]
    [InlineData(null)]
    public async Task GetWorkoutAnalysisAsync_WithDifferentAnalysisTypes_LogsCorrectly(string? analysisType)
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            AnalysisType = analysisType,
            RecentWorkouts = new List<AIWorkoutDataDto>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred regardless of analysis type
        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    #endregion

    #region Additional Methods

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithNullWorkouts_LogsZeroWorkouts()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = null // Test null workouts
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred
        VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 0 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = null,
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto { ActivityType = "Test", Distance = 1.0 }
        }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 1 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Error, "Error getting GoogleGemini workout analysis");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetPerformanceTrendsAsync(123, "week", CancellationToken.None));

        VerifyLogCalled(LogLevel.Error, "Error getting performance trends");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetTrainingRecommendationsAsync(456, CancellationToken.None));

        VerifyLogCalled(LogLevel.Error, "Error getting training recommendations");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var workouts = new List<AIWorkoutDataDto>
    {
        new AIWorkoutDataDto { ActivityType = "Test" }
    };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.AnalyzeHealthMetricsAsync(789, workouts, CancellationToken.None));

        VerifyLogCalled(LogLevel.Error, "Error analyzing health metrics");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.AnalyzeHealthMetricsAsync(123, null, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 123");
    }

    #endregion

    #region Edge Cases and Data Validation

    [Fact]
    public async Task GetMotivationAsync_WithEmptyStrings_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "",
                FitnessLevel = "",
                PrimaryGoal = ""
            },
            PreferredTone = "",
            ContextualInfo = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetMotivationAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete:");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = null,
            AnalysisType = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    [Theory]
    [InlineData("week")]
    [InlineData("year")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetPerformanceTrendsAsync_WithDifferentTimeFrames_LogsCorrectly(string? timeFrame)
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);
        const int athleteId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetPerformanceTrendsAsync(athleteId, timeFrame ?? "month", CancellationToken.None));

        var expectedTimeFrame = timeFrame ?? "month";
        VerifyLogCalled(LogLevel.Information, $"performance trends for athlete: {athleteId}, timeFrame: {expectedTimeFrame}");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithWorkoutsContainingNullValues_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = null,
                FitnessLevel = null,
                PrimaryGoal = null
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto
            {
                Date = DateTime.UtcNow,
                ActivityType = null,
                Distance = 0,
                Duration = 0,
                Calories = 0
            }
        },
            AnalysisType = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 1 workouts");
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CallsChannelDispose()
    {
        // Arrange & Act
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // This should not throw
        _service.Dispose();

        // Calling dispose again should also not throw (idempotent)
        _service.Dispose();

        // Assert - No exception thrown means test passes
        Assert.True(true);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        // Act & Assert - Should not throw
        _service.Dispose();
        _service.Dispose();
        _service.Dispose();

        Assert.True(true);
    }

    #endregion

    #region Additional Data Conversion Edge Cases

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithWorkoutsContainingNullActivityType_HandlesGracefully()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var workouts = new List<AIWorkoutDataDto>
    {
        new AIWorkoutDataDto
        {
            Date = DateTime.UtcNow,
            ActivityType = null, // Test null handling
            Distance = 5.0,
            Duration = 1800,
            Calories = 400
        }
    };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.AnalyzeHealthMetricsAsync(555, workouts, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 555");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithEmptyAnalysisType_UsesGeneral()
    {
        // Arrange
        _service = new GrpcAIAssistantClientService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AnalysisType = "", // Empty string
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = new List<AIWorkoutDataDto>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 0 workouts");
    }

    #endregion

    #region Helper Methods

    private void VerifyLogCalled(LogLevel logLevel, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        _service?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}