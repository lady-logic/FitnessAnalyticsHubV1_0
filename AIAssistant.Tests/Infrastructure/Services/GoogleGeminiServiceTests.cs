namespace AIAssistant.Tests.Infrastructure.Services;

using System.Net;
using System.Text;
using System.Text.Json;
using AIAssistant.Tests.Helpers;
using FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

public class GoogleGeminiServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
    private readonly HttpClient httpClient;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<ILogger<GoogleGeminiService>> mockLogger;
    private readonly GoogleGeminiService service;

    public GoogleGeminiServiceTests()
    {
        this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        this.httpClient = new HttpClient(this.mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/"),
        };

        this.mockConfiguration = MockSetup.CreateMockConfiguration();
        this.mockLogger = MockSetup.CreateMockLogger<GoogleGeminiService>();

        this.service = new GoogleGeminiService(
            this.httpClient,
            this.mockConfiguration.Object,
            this.mockLogger.Object);
    }

    #region GetFitnessAnalysisAsync Tests

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithValidPrompt_ReturnsGeminiAnalysis()
    {
        // Arrange
        string prompt = "Analyze my 10K run in 45 minutes - is this good progress?";
        object geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
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

        this.SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

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
        string prompt = "Test fitness prompt";
        this.SetupHttpResponse(null, HttpStatusCode.Unauthorized);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("401", result);

        MockSetup.VerifyLog(this.mockLogger, LogLevel.Error, "UNAUTHORIZED");
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithRateLimitResponse_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Test fitness prompt";
        this.SetupHttpResponse(null, HttpStatusCode.TooManyRequests);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("429", result);

        MockSetup.VerifyLog(this.mockLogger, LogLevel.Warning, "RATE LIMITED");
    }

    [Fact]
    public async Task GetFitnessAnalysisAsync_WithTimeout_ReturnsFallbackResponse()
    {
        // Arrange
        string prompt = "Test fitness prompt";
        TaskCanceledException timeoutException = new TaskCanceledException("Timeout", new TimeoutException());

        this.mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(timeoutException);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("timeout", result);

        MockSetup.VerifyLog(this.mockLogger, LogLevel.Warning, "timeout");
    }

    #endregion

    #region GetHealthAnalysisAsync Tests

    [Fact]
    public async Task GetHealthAnalysisAsync_WithValidPrompt_ReturnsHealthFocusedAnalysis()
    {
        // Arrange
        string prompt = "I've been running 6 days a week and feeling very tired. Is this overtraining?";
        object geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
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

        this.SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetHealthAnalysisAsync(prompt, CancellationToken.None);

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
        string prompt = "Health analysis prompt";
        string malformedResponse = "{ invalid json response }";

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
        string result = await this.service.GetHealthAnalysisAsync(prompt, CancellationToken.None);

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
        string prompt = "I'm struggling to stay motivated for my marathon training. Help me!";
        object geminiResponse = MockSetup.CreateGoogleGeminiSuccessResponse(
            @"## 💪 MOTIVATION
Du trainierst für etwas Großartiges - einen Marathon! Diese Herausforderung wird dich stärker machen.

## ✨ HEUTE FOKUSSIEREN
• **Kleine Schritte** - Jeder Lauf bringt dich dem Ziel näher
• **Deine Fortschritte** - Du bist bereits weiter als beim Start
• **Das Warum** - Erinnere dich, warum du angefangen hast

## 🎯 ZIEL IM BLICK
Der Marathon wartet auf dich! Vertraue dem Prozess und genieße jeden Schritt der Reise. Du schaffst das! 🏃‍♂️");

        this.SetupHttpResponse(geminiResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetMotivationAsync(prompt, CancellationToken.None);

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
        string prompt = "Motivate me";
        this.mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        string result = await this.service.GetMotivationAsync(prompt, CancellationToken.None);

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
        Mock<IConfiguration> configWithoutKey = new Mock<IConfiguration>();
        configWithoutKey.Setup(c => c["GoogleAI:ApiKey"]).Returns((string?)null);
        GoogleGeminiService serviceWithoutKey = new GoogleGeminiService(
            this.httpClient,
            configWithoutKey.Object,
            this.mockLogger.Object);

        // Act
        string result = await serviceWithoutKey.GetFitnessAnalysisAsync("test", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("nicht verfügbar", result);

        // Prüfe, dass der Error-Log aufgerufen wurde
        MockSetup.VerifyLog(this.mockLogger, LogLevel.Error, "Error calling Google Gemini API");
    }

    [Fact]
    public async Task GoogleGeminiService_WithNullApiKey_ReturnsFallbackResponse()
    {
        // Arrange
        Mock<IConfiguration> configWithNullKey = new Mock<IConfiguration>();
        configWithNullKey.Setup(c => c["GoogleAI:ApiKey"]).Returns((string)null!);

        using HttpClient httpClient = new HttpClient();
        GoogleGeminiService serviceWithNullKey = new GoogleGeminiService(
            httpClient,
            configWithNullKey.Object,
            this.mockLogger.Object);

        // Act
        string result = await serviceWithNullKey.GetHealthAnalysisAsync("test", CancellationToken.None);

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
        string prompt = $"Test {modelType} prompt";
        object response = MockSetup.CreateGoogleGeminiSuccessResponse($"Response for {modelType}");

        this.SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        string result = await this.ExecuteServiceMethod(modelType, prompt);

        // Assert
        Assert.NotNull(result);

        // Verify the correct model was used in the request
        this.mockHttpMessageHandler.Protected().Verify(
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
            "fitness" => await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None),
            "health" => await this.service.GetHealthAnalysisAsync(prompt, CancellationToken.None),
            "motivation" => await this.service.GetMotivationAsync(prompt, CancellationToken.None),
            "analysis" => await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None), // Default to fitness for analysis
            _ => throw new ArgumentException($"Unknown model type: {modelType}")
        };
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GoogleGeminiService_HandlesEmptyResponseGracefully()
    {
        // Arrange
        string prompt = "Test prompt";
        var emptyResponse = new { candidates = Array.Empty<object>() };

        this.SetupHttpResponse(emptyResponse, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetFitnessAnalysisAsync(prompt, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        MockSetup.AssertIsFallbackResponse(result);
        Assert.Contains("unexpected_format", result);
    }

    [Fact]
    public async Task GoogleGeminiService_HandlesResponseWithoutTextProperty()
    {
        // Arrange
        string prompt = "Test prompt";
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
                            new { data = "no text property" },
                        },
                    },
                },
            },
        };

        this.SetupHttpResponse(responseWithoutText, HttpStatusCode.OK);

        // Act
        string result = await this.service.GetMotivationAsync(prompt, CancellationToken.None);

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
        string originalPrompt = "My 5K time is 25 minutes";
        object response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced fitness response");

        this.SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await this.service.GetFitnessAnalysisAsync(originalPrompt, CancellationToken.None);

        // Assert
        // Verify that the request contains enhanced prompt structure
        this.mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                this.ExtractRequestContent(req).Contains("Fitnessexperte") &&
                this.ExtractRequestContent(req).Contains("TRAININGSANALYSE") &&
                this.ExtractRequestContent(req).Contains(originalPrompt)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GoogleGeminiService_CreatesEnhancedPromptForHealth()
    {
        // Arrange
        string originalPrompt = "I feel tired after workouts";
        object response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced health response");

        this.SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await this.service.GetHealthAnalysisAsync(originalPrompt, CancellationToken.None);

        // Assert
        this.mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                this.ExtractRequestContent(req).Contains("Gesundheitsexperte") &&
                this.ExtractRequestContent(req).Contains("GESUNDHEITSANALYSE") &&
                this.ExtractRequestContent(req).Contains(originalPrompt)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GoogleGeminiService_CreatesEnhancedPromptForMotivation()
    {
        // Arrange
        string originalPrompt = "I need motivation to exercise";
        object response = MockSetup.CreateGoogleGeminiSuccessResponse("Enhanced motivation response");
        this.SetupHttpResponse(response, HttpStatusCode.OK);

        // Act
        await this.service.GetMotivationAsync(originalPrompt, CancellationToken.None);

        // Assert
        this.mockHttpMessageHandler.Protected().Verify(
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

    private string ExtractRequestContent(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return string.Empty;
        }

        string content = request.Content.ReadAsStringAsync().Result;
        return content;
    }

    #endregion

    public void Dispose()
    {
        this.httpClient?.Dispose();
    }
}