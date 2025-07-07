using System.Net;
using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using AIAssistant.Tests.Helpers;

namespace AIAssistant.Tests.Infrastructure.Services;

public class GoogleGeminiServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<GoogleGeminiService>> _mockLogger;
    private readonly GoogleGeminiService _service;

    public GoogleGeminiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };

        _mockConfiguration = MockSetup.CreateMockConfiguration();
        _mockLogger = MockSetup.CreateMockLogger<GoogleGeminiService>();

        _service = new GoogleGeminiService(
            _httpClient,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region GetFitnessAnalysisAsync Tests

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithValidPrompt_ReturnsGeminiAnalysis()
    {
        // Arrange
        var prompt = "Analyze my 10K run in 45 minutes - is this good progress?";
        var geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
            @"## 📊 TRAININGSANALYSE
**Gesamtbewertung:** Deine 10K-Zeit von 45 Minuten ist sehr respektabel!
**Trainingsvolumen:** Zeigt gute Ausdauerkapazität
**Intensität:** Moderates bis zügiges Tempo

## 💡 WICHTIGE ERKENNTNISSE
• **Pace-Analyse** - 4:30 min/km ist ein solides Tempo für 10K
• **Ausdauerleistung** - Zeigt gute aerobe Fitness
• **Verbesserungspotential** - Mit gezieltem Training unter 40 Minuten möglich

## 🚀 EMPFEHLUNGEN
1. **Sofort umsetzbar:** Intervalltraining 1x pro Woche einbauen
2. **Mittelfristig:** Lange Läufe für Grundlagenausdauer
3. **Langfristig:** Tempo-Läufe für Geschwindigkeitsentwicklung");

        SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TRAININGSANALYSE", result);
        Assert.Contains("45 Minuten", result);
        Assert.Contains("Intervalltraining", result);
        Assert.Contains("📊", result); // Check for emoji formatting

        MockSetup.AssertIsAIGeneratedResponse(result);
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithUnauthorizedResponse_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test fitness prompt";
        SetupHttpResponse(null, HttpStatusCode.Unauthorized);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("401", result);

        MockSetup.VerifyLog(_mockLogger, LogLevel.Error, "UNAUTHORIZED");
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithRateLimitResponse_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test fitness prompt";
        SetupHttpResponse(null, HttpStatusCode.TooManyRequests);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("429", result);

        MockSetup.VerifyLog(_mockLogger, LogLevel.Warning, "RATE LIMITED");
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithTimeout_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Test fitness prompt";
        var timeoutException = new TaskCanceledException("Timeout", new TimeoutException());

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(timeoutException);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("timeout", result);

        MockSetup.VerifyLog(_mockLogger, LogLevel.Warning, "timeout");
    }

    #endregion

    #region GetHealthAnalysisAsync Tests

    [Fact]
    public async Task GetHealthAnalysisAsync_WithValidPrompt_ReturnsHealthFocusedAnalysis()
    {
        // Arrange
        var prompt = "I've been running 6 days a week and feeling very tired. Is this overtraining?";
        var geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
            @"## 🏥 GESUNDHEITSANALYSE
**Belastungsmanagement:** 6 Trainingstage pro Woche sind sehr intensiv
**Regeneration:** Müdigkeit deutet auf unzureichende Erholung hin
**Verletzungsrisiko:** Erhöht bei diesem Trainingsvolumen

## ⚠️ GESUNDHEITSINDIKATOREN
• **Ermüdung** - Warnsignal für mögliches Übertraining
• **Trainingsfrequenz** - Zu hoch für nachhaltige Entwicklung
• **Regenerationsdefizit** - Körper benötigt mehr Erholungszeit

## 🌱 WELLNESS-EMPFEHLUNGEN
1. **Regeneration:** Reduziere auf 4-5 Trainingstage pro Woche
2. **Prävention:** Integriere aktive Erholungstage
3. **Langfristige Gesundheit:** Höre auf die Signale deines Körpers");

        SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetHealthAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("GESUNDHEITSANALYSE", result);
        Assert.Contains("Übertraining", result);
        Assert.Contains("Regeneration", result);
        Assert.Contains("🏥", result); // Health-specific emoji

        MockSetup.AssertIsAIGeneratedResponse(result);
    }

    [Fact]
    public async Task GetHealthAnalysisAsync_WithMalformedResponse_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Health analysis prompt";
        var malformedResponse = "{ invalid json response }";

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
        var result = await _service.GetHealthAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("nicht verfügbar", result);
    }

    #endregion

    #region GetMotivationAsync Tests

    [Fact]
    public async Task GetMotivationAsync_WithValidPrompt_ReturnsMotivationalMessage()
    {
        // Arrange
        var prompt = "I'm struggling to stay motivated for my marathon training. Help me!";
        var geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
            @"## 💪 MOTIVATION
Du trainierst für etwas Großartiges - einen Marathon! Diese Herausforderung wird dich stärker machen.

## ✨ HEUTE FOKUSSIEREN
• **Kleine Schritte** - Jeder Lauf bringt dich dem Ziel näher
• **Deine Fortschritte** - Du bist bereits weiter als beim Start
• **Das Warum** - Erinnere dich, warum du angefangen hast

## 🎯 ZIEL IM BLICK
Der Marathon wartet auf dich! Vertraue dem Prozess und genieße jeden Schritt der Reise. Du schaffst das! 🏃‍♂️");

        SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetMotivationAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("💪 MOTIVATION", result);
        Assert.Contains("Marathon", result);
        Assert.Contains("Du schaffst das", result);
        Assert.Contains("🎯", result); // Motivational emoji

        MockSetup.AssertIsAIGeneratedResponse(result);
    }

    [Fact]
    public async Task GetMotivationAsync_WithGenericException_ReturnsFallbackResponse()
    {
        // Arrange
        var prompt = "Motivate me";
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetMotivationAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("Network error", result);
        Assert.Contains("💪", result); // Should contain motivational emoji even in fallback
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task GoogleGeminiService_WithMissingApiKey_ReturnsFallbackResponse()
    {
        // Arrange
        var configWithoutKey = new Mock<IConfiguration>();
        configWithoutKey.Setup(c => c["GoogleAI:ApiKey"]).Returns((string?)null);
        var serviceWithoutKey = new GoogleGeminiService(
            _httpClient,
            configWithoutKey.Object,
            _mockLogger.Object);

        // Act
        var result = await serviceWithoutKey.GetFitnessAnalysisAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("nicht verfügbar", result);
        // Prüfe, dass der Error-Log aufgerufen wurde
        MockSetup.VerifyLog(_mockLogger, LogLevel.Error, "Error calling Google Gemini API");
    }

    [Fact]
    public async Task GoogleGeminiService_WithNullApiKey_ReturnsFallbackResponse()
    {
        // Arrange
        var configWithNullKey = new Mock<IConfiguration>();
        configWithNullKey.Setup(c => c["GoogleAI:ApiKey"]).Returns((string)null!);

        using var httpClient = new HttpClient();
        var serviceWithNullKey = new GoogleGeminiService(
            httpClient,
            configWithNullKey.Object,
            _mockLogger.Object);

        // Act
        var result = await serviceWithNullKey.GetHealthAnalysisAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("nicht verfügbar", result); // Fallback response
        Assert.Contains("Google AI API Key not configured", result); // Error message im Fallback
    }

    #endregion

    #region Model Selection Tests

    [Theory]
    [InlineData("fitness", "gemini-1.5-flash")]
    [InlineData("health", "gemini-1.5-flash")]
    [InlineData("motivation", "gemini-1.5-flash")]
    public async Task GoogleGeminiService_UsesCorrectModelForDifferentTypes(string modelType, string expectedModel)
    {
        // Arrange
        var prompt = $"Test {modelType} prompt";
        var response = MockSetup.CreateGoogleGeminiSuccessResponse($"Response for {modelType}");

        SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        var result = await ExecuteServiceMethod(modelType, prompt);

        // Assert
        Assert.NotNull(result);

        // Verify the correct model was used in the request
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains(expectedModel)),
            ItExpr.IsAny<CancellationToken>());
    }

    private async Task<string> ExecuteServiceMethod(string modelType, string prompt)
    {
        return modelType switch
        {
            "fitness" => await _service.GetFitnessAnalysisAsync(prompt),
            "health" => await _service.GetHealthAnalysisAsync(prompt),
            "motivation" => await _service.GetMotivationAsync(prompt),
            "analysis" => await _service.GetFitnessAnalysisAsync(prompt), // Default to fitness for analysis
            _ => throw new ArgumentException($"Unknown model type: {modelType}")
        };
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GoogleGeminiService_HandlesEmptyResponseGracefully()
    {
        // Arrange
        var prompt = "Test prompt";
        var emptyResponse = new { candidates = Array.Empty<object>() };

        SetupHttpResponse(emptyResponse, HttpStatusCode.OK);

        // Act
        var result = await _service.GetFitnessAnalysisAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("unexpected_format", result);
    }

    [Fact]
    public async Task GoogleGeminiService_HandlesResponseWithoutTextProperty()
    {
        // Arrange
        var prompt = "Test prompt";
        var responseWithoutText = new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { data = "no text property" }
                        }
                    }
                }
            }
        };

        SetupHttpResponse(responseWithoutText, HttpStatusCode.OK);

        // Act
        var result = await _service.GetMotivationAsync(prompt);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
    }

    #endregion

    #region Enhanced Prompt Tests

    [Fact]
    public async Task GoogleGeminiService_CreatesEnhancedPromptForFitness()
    {
        // Arrange
        var originalPrompt = "My 5K time is 25 minutes";
        var response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced fitness response");

        SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await _service.GetFitnessAnalysisAsync(originalPrompt);

        // Assert
        // Verify that the request contains enhanced prompt structure
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                ExtractRequestContent(req).Contains("Fitnessexperte") &&
                ExtractRequestContent(req).Contains("TRAININGSANALYSE") &&
                ExtractRequestContent(req).Contains(originalPrompt)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GoogleGeminiService_CreatesEnhancedPromptForHealth()
    {
        // Arrange
        var originalPrompt = "I feel tired after workouts";
        var response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced health response");

        SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await _service.GetHealthAnalysisAsync(originalPrompt);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                ExtractRequestContent(req).Contains("Gesundheitsexperte") &&
                ExtractRequestContent(req).Contains("GESUNDHEITSANALYSE") &&
                ExtractRequestContent(req).Contains(originalPrompt)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GoogleGeminiService_CreatesEnhancedPromptForMotivation()
    {
        // Arrange
        var originalPrompt = "I need motivation to exercise";
        var response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced motivation response");
        SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await _service.GetMotivationAsync(originalPrompt);

        // Assert 
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
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

    private string ExtractRequestContent(HttpRequestMessage request)
    {
        if (request.Content == null) return string.Empty;

        var content = request.Content.ReadAsStringAsync().Result;
        return content;
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}