﻿namespace AIAssistant.Tests.Infrastructure.Services;

using System.Net;
using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

public class HuggingFaceServiceTests : IDisposable
{
    private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
    private readonly HttpClient httpClient;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<ILogger<HuggingFaceService>> mockLogger;
    private readonly HuggingFaceService service;

    public HuggingFaceServiceTests()
    {
        this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
        this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        this.httpClient = new HttpClient(this.mockHttpMessageHandler.Object);
        this.mockConfiguration = new Mock<IConfiguration>();
        this.mockLogger = new Mock<ILogger<HuggingFaceService>>();

        // Setup Configuration
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"])
                         .Returns("test_api_key");

        this.mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                             .Returns(this.httpClient);

        this.service = new HuggingFaceService(
            this.httpClient,
            this.mockConfiguration.Object,
            this.mockLogger.Object);
    }

    #region GetFitnessAnalysisAsync Tests

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithValidPrompt_ReturnsAnalysis()
    {
        // Arrange
        string prompt = "Analyze my recent 5K run with time 25:30";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Your 5K time of 25:30 shows solid fitness. Focus on interval training to improve pace.",
                    },
                },
            },
        };

        this.SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("25:30", result);
        Assert.Contains("interval training", result);
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithApiError_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Test prompt";
        this.SetupHttpResponse(null, HttpStatusCode.Unauthorized);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("Unauthorized", result); // HTTP Status als String
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithTimeout_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Test prompt";
        this.mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", new TimeoutException()));

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("timeout", result); // Der errorType wird als "timeout" übergeben
    }

    #endregion

    #region GetMotivationAsync Tests

    [Fact]
    public async Task GetMotivationAsync_WithValidPrompt_ReturnsMotivation()
    {
        // Arrange
        string prompt = "I need motivation for my workout today";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "You've got this! Every workout makes you stronger. Push through today!",
                    },
                },
            },
        };

        this.SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetMotivationAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("You've got this", result);
        Assert.Contains("stronger", result);
    }

    [Fact]
    public async Task GetMotivationAsync_WithRateLimitError_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Motivate me";
        this.SetupHttpResponse(null, HttpStatusCode.TooManyRequests);

        // Act
        string result = await this.service.GetMotivationAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Keep pushing forward", result); // Englischer Text
        Assert.Contains("💪", result);
        Assert.Contains("You've got this", result);
    }

    #endregion

    #region GetHealthAnalysisAsync Tests

    [Fact]
    public async Task GetHealthAnalysisAsync_WithValidPrompt_ReturnsHealthAnalysis()
    {
        // Arrange
        string prompt = "Analyze my training load: 5 runs this week, feeling tired";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Your training volume is high. Consider adding rest days to prevent overtraining.",
                    },
                },
            },
        };

        this.SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetHealthAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("training volume", result);
        Assert.Contains("rest days", result);
    }

    #endregion

    #region Error Handling Tests

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task HuggingFaceService_WithHttpErrors_ReturnsFallbackResponse(HttpStatusCode statusCode)
    {
        // Arrange
        string prompt = "Test prompt";
        this.SetupHttpResponse(null, statusCode);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("workout data", result); // Teil des fitness fallback
    }

    [Fact]
    public async Task HuggingFaceService_WithMalformedResponse_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Test prompt";
        string malformedResponse = "{ invalid json";
        HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedResponse, Encoding.UTF8, "application/json"),
        };
        this.mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("workout data", result); // Teil des fitness fallback
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void HuggingFaceService_WithMissingApiKey_LogsWarning()
    {
        // Arrange
        Mock<IConfiguration> mockConfigNoKey = new Mock<IConfiguration>();
        mockConfigNoKey.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        HuggingFaceService serviceWithoutKey = new HuggingFaceService(
            this.httpClient,
            mockConfigNoKey.Object,
            this.mockLogger.Object);

        // Assert
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No HuggingFace API Key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse(object? responseObject, HttpStatusCode statusCode)
    {
        HttpResponseMessage httpResponse;

        if (responseObject != null)
        {
            string jsonResponse = JsonSerializer.Serialize(responseObject);
            httpResponse = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };
        }
        else
        {
            httpResponse = new HttpResponseMessage(statusCode);
        }

        this.mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    #endregion

    public void Dispose()
    {
        this.httpClient?.Dispose();
    }
}