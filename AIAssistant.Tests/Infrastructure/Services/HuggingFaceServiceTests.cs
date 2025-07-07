using System.Net;
using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace AIAssistant.Tests.Infrastructure.Services;

public class HuggingFaceServiceTests : IDisposable
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<HuggingFaceService>> _mockLogger;
    private readonly HuggingFaceService _service;

    public HuggingFaceServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<HuggingFaceService>>();

        // Setup Configuration
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"])
                         .Returns("test_api_key");

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                             .Returns(_httpClient);

        _service = new HuggingFaceService(
            _httpClient,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region GetFitnessAnalysisAsync Tests

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithValidPrompt_ReturnsAnalysis()
    {
        // Arrange
        var prompt = "Analyze my recent 5K run with time 25:30";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Your 5K time of 25:30 shows solid fitness. Focus on interval training to improve pace."
                    }
                }
            }
        };

        SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("25:30", result);
        Assert.Contains("interval training", result);
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithApiError_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test prompt";
        SetupHttpResponse(null, HttpStatusCode.Unauthorized);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("Unauthorized", result); // HTTP Status als String
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithTimeout_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test prompt";
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", new TimeoutException()));

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

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
        var prompt = "I need motivation for my workout today";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "You've got this! Every workout makes you stronger. Push through today!"
                    }
                }
            }
        };

        SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetMotivationAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("You've got this", result);
        Assert.Contains("stronger", result);
    }

    [Fact]
    public async Task GetMotivationAsync_WithRateLimitError_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Motivate me";
        SetupHttpResponse(null, HttpStatusCode.TooManyRequests);

        // Act
        var result = await _service.GetMotivationAsync(prompt);

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
        var prompt = "Analyze my training load: 5 runs this week, feeling tired";
        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Your training volume is high. Consider adding rest days to prevent overtraining."
                    }
                }
            }
        };

        SetupHttpResponse(expectedResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetHealthAnalysisAsync(prompt);

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
        var prompt = "Test prompt";
        SetupHttpResponse(null, statusCode);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("temporarily unavailable", result); // Englischer Text
        Assert.Contains("workout data", result); // Teil des fitness fallback
    }

    [Fact]
    public async Task HuggingFaceService_WithMalformedResponse_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test prompt";
        var malformedResponse = "{ invalid json";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedResponse, Encoding.UTF8, "application/json")
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

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
        var mockConfigNoKey = new Mock<IConfiguration>();
        mockConfigNoKey.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        var serviceWithoutKey = new HuggingFaceService(
            _httpClient,
            mockConfigNoKey.Object,
            _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
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
            var jsonResponse = JsonSerializer.Serialize(responseObject);
            httpResponse = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };
        }
        else
        {
            httpResponse = new HttpResponseMessage(statusCode);
        }

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}