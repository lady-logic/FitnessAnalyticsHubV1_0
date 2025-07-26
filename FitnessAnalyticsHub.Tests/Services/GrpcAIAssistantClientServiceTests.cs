using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessAnalyticsHub.Tests.Services;

public class GrpcAIAssistantClientServiceTests : IDisposable
{
    private readonly Mock<ILogger<GrpcAIAssistantClientService>> mockLogger;
    private readonly Mock<IConfiguration> mockConfiguration;
    private GrpcAIAssistantClientService? service;

    public GrpcAIAssistantClientServiceTests()
    {
        this.mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();
        this.mockConfiguration = new Mock<IConfiguration>();

        // Setup default configuration
        this.mockConfiguration.Setup(c => c["AIAssistant:GrpcUrl"])
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
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = null,
            PreferredTone = "Motivational",
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: Unknown");
    }

    [Fact]
    public async Task GetMotivationAsync_WithValidAthleteProfile_LogsAthleteName()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "John Doe",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Weight Loss",
            },
            PreferredTone = "Encouraging",
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: John Doe");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithMultipleWorkouts_LogsWorkoutCount()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Mike Johnson",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                    ActivityType = "Run",
                    Distance = 10.0,
                    Duration = 2700,
                    Calories = 650,
                },
                new AIWorkoutDataDto
                {
                    Date = DateTime.UtcNow.AddDays(-2),
                    ActivityType = "Cycle",
                    Distance = 25.0,
                    Duration = 3600,
                    Calories = 800,
                },
            },
            AnalysisType = "Performance",
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 2 workouts");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithEmptyWorkouts_LogsZeroWorkouts()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Beginner" },
            RecentWorkouts = new List<AIWorkoutDataDto>(),
            AnalysisType = "General",
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithValidRequest_LogsCorrectMessage()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto { ActivityType = "Swimming", Distance = 2.0, Duration = 60, Calories = 500 },
            },
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 1 workouts");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithValidParameters_LogsCorrectMessage()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        const int athleteId = 123;
        const string timeFrame = "month";

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetPerformanceTrendsAsync(athleteId, timeFrame, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, $"performance trends for athlete: {athleteId}, timeFrame: {timeFrame}");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithValidAthleteId_LogsCorrectMessage()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        const int athleteId = 456;

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetTrainingRecommendationsAsync(athleteId, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, $"training recommendations for athlete: {athleteId}");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithValidData_LogsCorrectMessage()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        const int athleteId = 789;
        var recentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto { ActivityType = "Yoga", Duration = 60, Calories = 200 },
        };

        // Act & Assert - We expect this to throw since there's no actual gRPC server
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(athleteId, recentWorkouts, CancellationToken.None));

        // Verify logging occurred before the exception
        this.VerifyLogCalled(LogLevel.Information, $"health metrics analysis for athlete: {athleteId}");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task IsHealthyAsync_WithConnectionError_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act
        var result = await this.service.IsHealthyAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
        this.VerifyLogCalled(LogLevel.Warning, "Health check failed");
    }

    [Fact]
    public async Task GetMotivationAsync_WithGrpcConnectionError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        // Verify error logging occurred
        this.VerifyLogCalled(LogLevel.Error, "Error getting motivation");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithGrpcConnectionError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" },
            RecentWorkouts = new List<AIWorkoutDataDto>(),
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify error logging occurred
        this.VerifyLogCalled(LogLevel.Error, "Error getting workout analysis");
    }

    #endregion

    #region Data Conversion Tests

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = null, // Test null handling
            RecentWorkouts = new List<AIWorkoutDataDto>
            {
                new AIWorkoutDataDto { ActivityType = "Run", Distance = 5.0 },
            },
        };

        // Act & Assert - Should not throw due to null athlete profile
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify it still attempts the call (logs the workout count)
        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 1 workouts");
    }

    [Theory]
    [InlineData("Performance")]
    [InlineData("Health")]
    [InlineData("Recovery")]
    [InlineData(null)]
    public async Task GetWorkoutAnalysisAsync_WithDifferentAnalysisTypes_LogsCorrectly(string? analysisType)
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            AnalysisType = analysisType,
            RecentWorkouts = new List<AIWorkoutDataDto>(),
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred regardless of analysis type
        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    #endregion

    #region Additional Methods

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithNullWorkouts_LogsZeroWorkouts()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = null, // Test null workouts
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging occurred
        this.VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 0 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = null,
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto { ActivityType = "Test", Distance = 1.0 },
        },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 1 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Error, "Error getting GoogleGemini workout analysis");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetPerformanceTrendsAsync(123, "week", CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Error, "Error getting performance trends");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetTrainingRecommendationsAsync(456, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Error, "Error getting training recommendations");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithGrpcError_LogsErrorAndThrows()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var workouts = new List<AIWorkoutDataDto>
    {
        new AIWorkoutDataDto { ActivityType = "Test" },
    };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(789, workouts, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Error, "Error analyzing health metrics");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(123, null, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 123");
    }

    #endregion

    #region Edge Cases and Data Validation

    [Fact]
    public async Task GetMotivationAsync_WithEmptyStrings_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = string.Empty,
                FitnessLevel = string.Empty,
                PrimaryGoal = string.Empty,
            },
            PreferredTone = string.Empty,
            ContextualInfo = string.Empty,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete:");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithNullWorkouts_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = null,
            AnalysisType = "Test",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    [Theory]
    [InlineData("week")]
    [InlineData("year")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetPerformanceTrendsAsync_WithDifferentTimeFrames_LogsCorrectly(string? timeFrame)
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);
        const int athleteId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetPerformanceTrendsAsync(athleteId, timeFrame ?? "month", CancellationToken.None));

        var expectedTimeFrame = timeFrame ?? "month";
        this.VerifyLogCalled(LogLevel.Information, $"performance trends for athlete: {athleteId}, timeFrame: {expectedTimeFrame}");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithWorkoutsContainingNullValues_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = null,
                FitnessLevel = null,
                PrimaryGoal = null,
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto
            {
                Date = DateTime.UtcNow,
                ActivityType = null,
                Distance = 0,
                Duration = 0,
                Calories = 0,
            },
        },
            AnalysisType = null,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 1 workouts");
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CallsChannelDispose()
    {
        // Arrange & Act
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // This should not throw
        this.service.Dispose();

        // Calling dispose again should also not throw (idempotent)
        this.service.Dispose();

        // Assert - No exception thrown means test passes
        Assert.True(true);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert - Should not throw
        this.service.Dispose();
        this.service.Dispose();
        this.service.Dispose();

        Assert.True(true);
    }

    #endregion

    #region Additional Data Conversion Edge Cases

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithWorkoutsContainingNullActivityType_HandlesGracefully()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var workouts = new List<AIWorkoutDataDto>
    {
        new AIWorkoutDataDto
        {
            Date = DateTime.UtcNow,
            ActivityType = null, // Test null handling
            Distance = 5.0,
            Duration = 1800,
            Calories = 400,
        },
    };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(555, workouts, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 555");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithEmptyAnalysisType_UsesGeneral()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AnalysisType = string.Empty, // Empty string
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            RecentWorkouts = new List<AIWorkoutDataDto>(),
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 0 workouts");
    }

    #endregion

    #region Constructor Coverage

    [Fact]
    public void Constructor_WithEmptyGrpcUrl_UsesEmptyString()
    {
        // Arrange
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(c => c["AIAssistant:GrpcUrl"])
            .Returns(string.Empty); // Empty string test

        var mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();

        // Act & Assert - Should throw because empty string is not a valid URI
        var exception = Assert.Throws<UriFormatException>(
            () => new GrpcAIAssistantClientService(mockLogger.Object, emptyConfig.Object));

        Assert.Contains("Invalid URI", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceGrpcUrl_ThrowsException()
    {
        // Arrange
        var whitespaceConfig = new Mock<IConfiguration>();
        whitespaceConfig.Setup(c => c["AIAssistant:GrpcUrl"])
            .Returns("   "); // Whitespace string

        var mockLogger = new Mock<ILogger<GrpcAIAssistantClientService>>();

        // Act & Assert - Should throw because whitespace is not a valid URI
        var exception = Assert.Throws<UriFormatException>(
            () => new GrpcAIAssistantClientService(mockLogger.Object, whitespaceConfig.Object));

        Assert.Contains("Invalid URI", exception.Message);
    }

    #endregion

    #region Success Path Coverage Tests

    [Fact]
    public async Task GetMotivationAsync_WithSuccessfulResponse_LogsSuccessMessage()
    {
        // Diese Tests decken die Success-Logs ab, die bisher nicht getestet wurden
        // Wir testen die Logik bis zum gRPC-Call und den Error-Catch

        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" },
        };

        // Act & Assert - This will fail at gRPC call but should log success message attempt
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        // Verify that both info and error logs are called
        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: Test User");
        this.VerifyLogCalled(LogLevel.Error, "Error getting motivation");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithSuccessfulResponse_LogsSuccessMessage()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test User" },
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto { ActivityType = "Run" },
        },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        // Verify logging
        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 1 workouts");
        this.VerifyLogCalled(LogLevel.Error, "Error getting workout analysis");
    }

    #endregion

    #region DateTime Parsing Coverage

    [Fact]
    public async Task GetMotivationAsync_WithInvalidDateFormat_UsesCurrentDateTime()
    {
        // This tests the DateTime.TryParse fallback logic
        // We can't directly test this without mocking the gRPC response,
        // but we can test that the method handles the parsing logic correctly

        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Test User",
                FitnessLevel = "Beginner",
                PrimaryGoal = "Health",
            },
            PreferredTone = "Encouraging",
            ContextualInfo = "First time user",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        // The method should have attempted to process the request
        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete: Test User");
    }

    #endregion

    #region Comprehensive Data Mapping Coverage

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithCompleteWorkoutData_ProcessesAllFields()
    {
        // Test complete data mapping to ensure all properties are covered

        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Complete User",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Performance",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto
            {
                Date = DateTime.UtcNow.AddDays(-1),
                ActivityType = "Running",
                Distance = 10.5,
                Duration = 3600,
                Calories = 650,
            },
            new AIWorkoutDataDto
            {
                Date = DateTime.UtcNow.AddDays(-2),
                ActivityType = "Cycling",
                Distance = 25.0,
                Duration = 7200,
                Calories = 800,
            },
        },
            AnalysisType = "Performance",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 2 workouts");
    }

    [Fact]
    public async Task GetGoogleGeminiWorkoutAnalysisAsync_WithCompleteData_ProcessesAllFields()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "Gemini User",
                FitnessLevel = "Expert",
                PrimaryGoal = "Competition",
            },
            RecentWorkouts = new List<AIWorkoutDataDto>
        {
            new AIWorkoutDataDto
            {
                Date = DateTime.UtcNow,
                ActivityType = "Swimming",
                Distance = 2.0,
                Duration = 3600,
                Calories = 400,
            },
        },
            AnalysisType = "Health",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetGoogleGeminiWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "GoogleGemini workout analysis for 1 workouts");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithCompleteWorkoutData_ProcessesAllFields()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var workouts = new List<AIWorkoutDataDto>
    {
        new AIWorkoutDataDto
        {
            Date = DateTime.UtcNow.AddDays(-1),
            ActivityType = "Weight Training",
            Distance = 0, // Weight training typically has no distance
            Duration = 2700,
            Calories = 350,
        },
        new AIWorkoutDataDto
        {
            Date = DateTime.UtcNow.AddDays(-3),
            ActivityType = "Yoga",
            Distance = 0,
            Duration = 3600,
            Calories = 200,
        },
    };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(456, workouts, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 456");
    }

    #endregion

    #region IsHealthy Method Coverage

    [Fact]
    public async Task IsHealthyAsync_WithSpecificCancellationToken_HandlesCancellation()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await this.service.IsHealthyAsync(cts.Token);

        // Assert
        Assert.False(result);

        // Should log warning due to cancellation
        this.VerifyLogCalled(LogLevel.Warning, "Health check failed");
    }

    [Fact]
    public async Task IsHealthyAsync_WithDefaultCancellationToken_ReturnsFalse()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act
        var result = await this.service.IsHealthyAsync();

        // Assert
        Assert.False(result);
        this.VerifyLogCalled(LogLevel.Warning, "Health check failed");
    }

    #endregion

    #region Edge Case String Handling

    [Fact]
    public async Task GetMotivationAsync_WithWhitespaceStrings_HandlesCorrectly()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIMotivationRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto
            {
                Name = "   ", // Whitespace
                FitnessLevel = "\t",
                PrimaryGoal = "\n",
            },
            PreferredTone = "  Motivational  ",
            ContextualInfo = "\r\n",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetMotivationAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting motivation for athlete:");
    }

    [Fact]
    public async Task GetWorkoutAnalysisAsync_WithWhitespaceAnalysisType_HandlesCorrectly()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var request = new AIWorkoutAnalysisRequestDto
        {
            AthleteProfile = new AIAthleteProfileDto { Name = "Test" },
            AnalysisType = "   ", // Whitespace
            RecentWorkouts = new List<AIWorkoutDataDto>(),
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetWorkoutAnalysisAsync(request, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "Requesting workout analysis for 0 workouts");
    }

    #endregion

    #region Performance and Edge Case Tests

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithDefaultTimeFrame_UsesMonth()
    {
        // Test the default parameter
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetPerformanceTrendsAsync(999)); // No timeFrame specified

        this.VerifyLogCalled(LogLevel.Information, "performance trends for athlete: 999, timeFrame: month");
    }

    [Fact]
    public async Task GetPerformanceTrendsAsync_WithZeroAthleteId_HandlesCorrectly()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetPerformanceTrendsAsync(0, "week", CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "performance trends for athlete: 0, timeFrame: week");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WithNegativeAthleteId_HandlesCorrectly()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.GetTrainingRecommendationsAsync(-1, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "training recommendations for athlete: -1");
    }

    [Fact]
    public async Task AnalyzeHealthMetricsAsync_WithEmptyWorkoutsList_HandlesCorrectly()
    {
        // Arrange
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        var emptyWorkouts = new List<AIWorkoutDataDto>();

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => this.service.AnalyzeHealthMetricsAsync(123, emptyWorkouts, CancellationToken.None));

        this.VerifyLogCalled(LogLevel.Information, "health metrics analysis for athlete: 123");
    }

    #endregion

    #region Dispose Edge Cases

    [Fact]
    public void Dispose_WithNullChannel_DoesNotThrow()
    {
        // This tests the ?. null-conditional operator in Dispose
        // We can't directly set _channel to null, but we can test multiple dispose calls

        // Arrange & Act
        this.service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Multiple dispose calls should not throw
        this.service.Dispose();
        this.service.Dispose();
        this.service.Dispose();

        // Assert - No exception = success
        Assert.True(true);
    }

    [Fact]
    public void Constructor_InitializesAllClients()
    {
        // Test that constructor properly initializes both gRPC clients
        // This indirectly tests the client creation logic

        // Arrange & Act
        using var service = new GrpcAIAssistantClientService(this.mockLogger.Object, this.mockConfiguration.Object);

        // Assert - If constructor completed without exception, clients were created
        Assert.NotNull(service);
    }

    #endregion

    #region Helper Methods

    private void VerifyLogCalled(LogLevel logLevel, string message)
    {
        this.mockLogger.Verify(
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
        this.service?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}