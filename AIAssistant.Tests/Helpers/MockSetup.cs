using System.Net;
using System.Text;
using System.Text.Json;
using AIAssistant.Application.Interfaces;
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
                Content = new StringContent(json, Encoding.UTF8, "application/json")
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
            BaseAddress = new Uri("https://test.api.com/")
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
    /// Creates a mock IWorkoutAnalysisService with default successful responses.
    /// </summary>
    public static Mock<IWorkoutAnalysisService> CreateMockWorkoutAnalysisService()
    {
        var mock = new Mock<IWorkoutAnalysisService>();

        mock.Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<AIAssistant.Application.DTOs.WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(TestDataBuilder.WorkoutAnalysisResponse().WithProvider("HuggingFace").Build());

        mock.Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<AIAssistant.Application.DTOs.WorkoutAnalysisRequestDto>()))
            .ReturnsAsync(TestDataBuilder.WorkoutAnalysisResponse().WithProvider("GoogleGemini").Build());

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
                        content = content
                    }
                }
            }
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
                        }
                    }
                }
            }
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
                code = code
            }
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
}