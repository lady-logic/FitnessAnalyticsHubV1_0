using System.Net;
using System.Text;
using System.Text.Json;
using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace AIAssistant.Tests.Helpers;

/// <summary>
/// Helper class for setting up common mocks used across tests.
/// Provides consistent mock configurations and reduces test setup code.
/// </summary>
public static class MockSetup
{
    #region HTTP Client Mocks

    /// <summary>
    /// Creates a mock HttpMessageHandler that returns the specified response.
    /// </summary>
    public static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(
        object responseObject,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        HttpResponseMessage response;
        if (responseObject != null)
        {
            var json = JsonSerializer.Serialize(responseObject);
            response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }
        else
        {
            response = new HttpResponseMessage(statusCode);
        }

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HttpMessageHandler that throws an exception.
    /// </summary>
    public static Mock<HttpMessageHandler> CreateMockHttpMessageHandlerWithException(Exception exception)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HttpClient with the specified handler.
    /// </summary>
    public static HttpClient CreateMockHttpClient(Mock<HttpMessageHandler> mockHandler)
    {
        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.api.com/"),
        };
    }

    #endregion

    #region Configuration Mocks

    /// <summary>
    /// Creates a mock IConfiguration with default AIAssistant settings.
    /// </summary>
    public static Mock<IConfiguration> CreateMockConfiguration()
    {
        var mockConfig = new Mock<IConfiguration>();

        // HuggingFace settings
        mockConfig.Setup(c => c["HuggingFace:ApiKey"]).Returns("test_hf_key");
        mockConfig.Setup(c => c["HuggingFace:BaseUrl"]).Returns("https://api-inference.huggingface.co/models/");

        // Google AI settings
        mockConfig.Setup(c => c["GoogleAI:ApiKey"]).Returns("test_google_key");
        mockConfig.Setup(c => c["GoogleAI:BaseUrl"]).Returns("https://generativelanguage.googleapis.com/v1beta");

        // Default provider
        mockConfig.Setup(c => c["AI:DefaultProvider"]).Returns("GoogleGemini");

        return mockConfig;
    }

    /// <summary>
    /// Creates a mock IConfiguration with missing API keys.
    /// </summary>
    public static Mock<IConfiguration> CreateMockConfigurationWithoutKeys()
    {
        var mockConfig = new Mock<IConfiguration>();

        mockConfig.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);
        mockConfig.Setup(c => c["GoogleAI:ApiKey"]).Returns((string?)null);

        return mockConfig;
    }

    #endregion

    #region Logger Mocks

    /// <summary>
    /// Creates a mock ILogger that captures log calls.
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Verifies that a specific log level was called with a message containing the specified text.
    /// </summary>
    public static void VerifyLog<T>(Mock<ILogger<T>> mockLogger, LogLevel level, string containsText)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(containsText)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Service Mocks

    /// <summary>
    /// Creates a mock IMotivationCoachService with default successful responses.
    /// </summary>
    public static Mock<IMotivationCoachService> CreateMockMotivationCoachService()
    {
        var mock = new Mock<IMotivationCoachService>();

        mock.Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<AIAssistant.Applications.DTOs.MotivationRequestDto>()))
            .ReturnsAsync(TestDataBuilder.MotivationResponse().Build());

        return mock;
    }

    /// <summary>
    /// Creates a mock WorkoutAnalysisService with default setup
    /// </summary>
    public static Mock<IWorkoutAnalysisService> CreateMockWorkoutAnalysisService()
    {
        var mock = new Mock<IWorkoutAnalysisService>();

        // Setup default successful responses
        var defaultResponse = new WorkoutAnalysisResponseDto
        {
            Analysis = "Default analysis response",
            KeyInsights = new List<string> { "Default insight 1", "Default insight 2" },
            Recommendations = new List<string> { "Default recommendation 1", "Default recommendation 2" },
            GeneratedAt = DateTime.UtcNow,
            Provider = "Mock",
            RequestId = Guid.NewGuid().ToString(),
        };

        mock.Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(defaultResponse);

        mock.Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(new WorkoutAnalysisResponseDto
            {
                Analysis = "GoogleGemini analysis response",
                KeyInsights = new List<string> { "GoogleGemini insight 1", "GoogleGemini insight 2" },
                Recommendations = new List<string> { "GoogleGemini recommendation 1" },
                GeneratedAt = DateTime.UtcNow,
                Provider = "GoogleGemini",
                RequestId = Guid.NewGuid().ToString(),
            });

        return mock;
    }

    #endregion

    #region AI Response Mocks

    /// <summary>
    /// Creates a successful HuggingFace API response.
    /// </summary>
    public static object CreateHuggingFaceSuccessResponse(string content = "Test AI response content")
    {
        return new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = content,
                    },
                },
            },
        };
    }

    /// <summary>
    /// Creates a successful Google Gemini API response.
    /// </summary>
    public static object CreateGoogleGeminiSuccessResponse(string content = "Test AI response content")
    {
        return new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = content }
                        },
                    },
                },
            },
        };
    }

    /// <summary>
    /// Creates an error response object.
    /// </summary>
    public static object CreateErrorResponse(string error = "Test error", int code = 400)
    {
        return new
        {
            error = new
            {
                message = error,
                code = code,
            },
        };
    }

    #endregion

    #region Test Scenarios

    /// <summary>
    /// Sets up mocks for a successful AI service scenario.
    /// </summary>
    public static (Mock<HttpMessageHandler> httpHandler, Mock<IConfiguration> config, Mock<ILogger<T>> logger)
        SetupSuccessfulAIService<T>(string responseContent = "Successful AI response")
    {
        var httpHandler = CreateMockHttpMessageHandler(CreateHuggingFaceSuccessResponse(responseContent));
        var config = CreateMockConfiguration();
        var logger = CreateMockLogger<T>();

        return (httpHandler, config, logger);
    }

    /// <summary>
    /// Sets up mocks for a failing AI service scenario.
    /// </summary>
    public static (Mock<HttpMessageHandler> httpHandler, Mock<IConfiguration> config, Mock<ILogger<T>> logger)
        SetupFailingAIService<T>(HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
    {
        var httpHandler = CreateMockHttpMessageHandler(null, statusCode);
        var config = CreateMockConfiguration();
        var logger = CreateMockLogger<T>();

        return (httpHandler, config, logger);
    }

    /// <summary>
    /// Sets up mocks for a timeout scenario.
    /// </summary>
    public static (Mock<HttpMessageHandler> httpHandler, Mock<IConfiguration> config, Mock<ILogger<T>> logger)
        SetupTimeoutAIService<T>()
    {
        var timeoutException = new TaskCanceledException("Timeout", new TimeoutException());
        var httpHandler = CreateMockHttpMessageHandlerWithException(timeoutException);
        var config = CreateMockConfiguration();
        var logger = CreateMockLogger<T>();

        return (httpHandler, config, logger);
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Asserts that a fallback response was returned (contains fallback indicators).
    /// </summary>
    public static void AssertIsFallbackResponse(string response)
    {
        Assert.NotNull(response);
        Assert.True(
            response.Contains("temporarily unavailable") ||
            response.Contains("vorübergehend nicht verfügbar") ||
            response.Contains("fallback") ||
            response.Contains("service unavailable"),
            "Response should indicate fallback behavior");
    }

    /// <summary>
    /// Asserts that a response contains AI-generated content (not a fallback).
    /// </summary>
    public static void AssertIsAIGeneratedResponse(string response)
    {
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response));
        Assert.False(
            response.Contains("temporarily unavailable") ||
            response.Contains("vorübergehend nicht verfügbar"),
            "Response should not be a fallback message");
    }

    #endregion

    #region Service Mocks Extensions

    /// <summary>
    /// Creates a mock WorkoutAnalysisService that throws exceptions
    /// </summary>
    public static Mock<IWorkoutAnalysisService> CreateFailingMockWorkoutAnalysisService()
    {
        var mock = new Mock<IWorkoutAnalysisService>();

        mock.Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("HuggingFace service error"));

        mock.Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>()))
            .ThrowsAsync(new Exception("GoogleGemini service error"));

        return mock;
    }

    #endregion

    #region Additional Service Mocks

    /// <summary>
    /// Creates a mock MotivationCoachService with enhanced setup
    /// </summary>
    public static Mock<IMotivationCoachService> CreateEnhancedMockMotivationCoachService()
    {
        var mock = new Mock<IMotivationCoachService>();

        var defaultResponse = new MotivationResponseDto
        {
            MotivationalMessage = "You're doing great! Keep pushing forward!",
            Quote = "Success is the sum of small efforts repeated day in and day out.",
            ActionableTips = new List<string>
            {
                "Set small achievable goals",
                "Focus on consistency",
                "Celebrate small wins",
            },
            GeneratedAt = DateTime.UtcNow,
        };

        mock.Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>()))
            .ReturnsAsync(defaultResponse);

        return mock;
    }

    #endregion

    #region Configuration Mocks Extensions

    /// <summary>
    /// Creates enhanced mock configuration with all necessary AI service settings
    /// </summary>
    public static Mock<IConfiguration> CreateEnhancedMockConfiguration()
    {
        var mockConfig = CreateMockConfiguration(); // Use existing method as base

        // Environment Configuration
        mockConfig.Setup(c => c["ASPNETCORE_ENVIRONMENT"]).Returns("Development");

        // Mock HuggingFace section
        var mockHuggingFaceSection = new Mock<IConfigurationSection>();
        var mockHuggingFaceChildren = new List<IConfigurationSection>();
        mockHuggingFaceSection.Setup(s => s.GetChildren()).Returns(mockHuggingFaceChildren);
        mockConfig.Setup(c => c.GetSection("HuggingFace")).Returns(mockHuggingFaceSection.Object);

        return mockConfig;
    }

    #endregion

    #region Test Data Builders

    /// <summary>
    /// Creates test workout data for various scenarios
    /// </summary>
    public static List<WorkoutDataDto> CreateTestWorkoutData(int count = 3)
    {
        var workouts = new List<WorkoutDataDto>();

        for (int i = 0; i < count; i++)
        {
            workouts.Add(new WorkoutDataDto
            {
                Date = DateTime.Now.AddDays(-(i + 1)),
                ActivityType = i % 2 == 0 ? "Run" : "Bike",
                Distance = 5.0 + i,
                Duration = 1800 + (i * 300),
                Calories = 350 + (i * 50),
                MetricsData = new Dictionary<string, double>
                {
                    { "heartRate", 140 + i },
                    { "pace", 5.0 - (i * 0.1) },
                },
            });
        }

        return workouts;
    }

    /// <summary>
    /// Creates test athlete profile data
    /// </summary>
    public static AthleteProfileDto CreateTestAthleteProfile(string name = "Test User", int id = 123)
    {
        return new AthleteProfileDto
        {
            Id = id.ToString(),
            Name = name,
            FitnessLevel = "Intermediate",
            PrimaryGoal = "Endurance Improvement",
            Preferences = new Dictionary<string, object>
            {
                { "preferredActivities", new[] { "Run", "Bike" } },
                { "trainingDays", 4 },
                { "intensityPreference", "moderate" },
            },
        };
    }

    /// <summary>
    /// Creates test health analysis request
    /// </summary>
    public static HealthAnalysisRequestDto CreateTestHealthAnalysisRequest(int athleteId = 123)
    {
        return new HealthAnalysisRequestDto
        {
            AthleteId = athleteId,
            RecentWorkouts = CreateTestWorkoutData(),
            HealthMetrics = new Dictionary<string, object>
            {
                { "restingHeartRate", 60 },
                { "weight", 70.5 },
                { "sleepHours", 7.5 },
            },
            FocusAreas = new List<string> { "injury_prevention", "recovery" },
            KnownIssues = new List<string> { "previous_knee_injury" },
        };
    }

    /// <summary>
    /// Creates test motivation request
    /// </summary>
    public static MotivationRequestDto CreateTestMotivationRequest(string athleteName = "Test User")
    {
        return new MotivationRequestDto
        {
            AthleteProfile = CreateTestAthleteProfile(athleteName),
            LastWorkout = CreateTestWorkoutData(1).First(),
            UpcomingWorkoutType = "Run",
            IsStruggling = false,
        };
    }

    /// <summary>
    /// Creates test workout analysis request with various options
    /// </summary>
    public static WorkoutAnalysisRequestDto CreateTestWorkoutAnalysisRequest(
        string analysisType = "Performance",
        int workoutCount = 3,
        string athleteName = "Test User")
    {
        return new WorkoutAnalysisRequestDto
        {
            AnalysisType = analysisType,
            RecentWorkouts = CreateTestWorkoutData(workoutCount),
            AthleteProfile = CreateTestAthleteProfile(athleteName),
            AdditionalContext = new Dictionary<string, object>
            {
                { "testContext", "unit_test" },
                { "analysisType", analysisType },
                { "timestamp", DateTime.UtcNow },
            },
        };
    }

    #endregion

    #region Logger Verification Helpers

    /// <summary>
    /// Verifies that a logger was called with specific log level and message content
    /// </summary>
    public static void VerifyLoggerCalled<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel logLevel,
        string expectedMessageContent,
        Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessageContent)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            times);
    }

    /// <summary>
    /// Verifies that a logger was called with an error
    /// </summary>
    public static void VerifyLoggerCalledWithError<T>(
        Mock<ILogger<T>> mockLogger,
        string expectedMessageContent,
        Times times)
    {
        VerifyLoggerCalled(mockLogger, LogLevel.Error, expectedMessageContent, times);
    }

    /// <summary>
    /// Verifies that a logger was called with information
    /// </summary>
    public static void VerifyLoggerCalledWithInformation<T>(
        Mock<ILogger<T>> mockLogger,
        string expectedMessageContent,
        Times times)
    {
        VerifyLoggerCalled(mockLogger, LogLevel.Information, expectedMessageContent, times);
    }

    #endregion

    #region Enhanced Assertion Helpers

    /// <summary>
    /// Asserts that a WorkoutAnalysisResponse contains valid data
    /// </summary>
    public static void AssertValidWorkoutAnalysisResponse(WorkoutAnalysisResponseDto response, string expectedProvider = null)
    {
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Analysis));
        Assert.NotNull(response.KeyInsights);
        Assert.NotNull(response.Recommendations);
        Assert.True(response.GeneratedAt > DateTime.MinValue);

        if (!string.IsNullOrEmpty(expectedProvider))
        {
            Assert.Equal(expectedProvider, response.Provider);
        }
    }

    /// <summary>
    /// Asserts that a MotivationResponse contains valid data
    /// </summary>
    public static void AssertValidMotivationResponse(MotivationResponseDto response)
    {
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.MotivationalMessage));
        Assert.True(response.GeneratedAt > DateTime.MinValue);

        // Quote and ActionableTips can be null, but if present should not be empty
        if (response.Quote != null)
        {
            Assert.False(string.IsNullOrWhiteSpace(response.Quote));
        }

        if (response.ActionableTips != null)
        {
            Assert.All(response.ActionableTips, tip => Assert.False(string.IsNullOrWhiteSpace(tip)));
        }
    }

    /// <summary>
    /// Asserts that a response indicates a successful operation (not an error or fallback)
    /// </summary>
    public static void AssertSuccessfulResponse(string responseContent)
    {
        Assert.NotNull(responseContent);
        Assert.False(string.IsNullOrWhiteSpace(responseContent));
        Assert.DoesNotContain("error", responseContent.ToLower());
        Assert.DoesNotContain("failed", responseContent.ToLower());
        AssertIsAIGeneratedResponse(responseContent);
    }

    #endregion
}
