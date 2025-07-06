using System.Text;
using System.Text.Json;
using AIAssistant.Application.Interfaces;

namespace FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;

public class GoogleGeminiService : IAIPromptService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleGeminiService> _logger;

    // Google Gemini Modelle
    private readonly Dictionary<string, string> _models = new()
    {
        { "fitness", "gemini-1.5-flash" },      // Kostenlos, schnell
        { "health", "gemini-1.5-flash" },       // Kostenlos, schnell  
        { "motivation", "gemini-1.5-flash" },   // Kostenlos, schnell
        { "analysis", "gemini-1.5-pro" }       // Bessere Qualität (falls verfügbar)
    };

    public GoogleGeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleGeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Base URL für Google Gemini API
        _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    }

    public async Task<string> GetFitnessAnalysisAsync(string prompt)
    {
        return await GetGeminiCompletionAsync(prompt, "fitness");
    }

    public async Task<string> GetHealthAnalysisAsync(string prompt)
    {
        return await GetGeminiCompletionAsync(prompt, "health");
    }

    public async Task<string> GetMotivationAsync(string prompt)
    {
        return await GetGeminiCompletionAsync(prompt, "motivation");
    }

    private async Task<string> GetGeminiCompletionAsync(string prompt, string modelType)
    {
        var enhancedPrompt = CreateEnhancedPrompt(prompt, modelType);

        try
        {
            var model = _models.GetValueOrDefault(modelType, "gemini-1.5-flash");
            var apiKey = GetApiKey();

            _logger.LogInformation("Calling Google Gemini API with model: {Model} for type: {ModelType}",
                model, modelType);

            // Google Gemini API Request Format
            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = enhancedPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 1000,
                    topP = 0.95,
                    topK = 40
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Google Gemini API Endpoint
            var endpoint = $"v1beta/models/{model}:generateContent?key={apiKey}";

            _logger.LogDebug("Request URL: {Url}", endpoint);
            _logger.LogDebug("Request payload: {Payload}", jsonContent);

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("Response content: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Gemini API error: {StatusCode} - {Error}", response.StatusCode, responseContent);

                // Spezifische Fehlerbehandlung
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("UNAUTHORIZED: Check your Google AI API key!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("RATE LIMITED: Google Gemini rate limit exceeded");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogError("BAD REQUEST: Check your request format or API key");
                }

                return GetFallbackResponse(modelType, response.StatusCode.ToString());
            }

            // Parse Google Gemini Response
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseJson.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content_) &&
                    content_.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var text))
                    {
                        var result = text.GetString() ?? string.Empty;
                        _logger.LogInformation("Successfully received response from Google Gemini API");
                        return result.Trim();
                    }
                }
            }

            _logger.LogWarning("Unexpected response format from Google Gemini API");
            _logger.LogDebug("Full response: {Response}", responseContent);
            return GetFallbackResponse(modelType, "unexpected_format");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Google Gemini request timeout for model type: {ModelType}", modelType);
            return GetFallbackResponse(modelType, "timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API for {ModelType}", modelType);
            return GetFallbackResponse(modelType, ex.Message);
        }
    }

    private string GetApiKey()
    {
        var apiKey = _configuration["GoogleAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Google AI API Key not configured. Please set 'GoogleAI:ApiKey' in configuration.");
        }
        return apiKey;
    }

    private string CreateEnhancedPrompt(string originalPrompt, string modelType)
    {
        var systemContext = modelType.ToLower() switch
        {
            "motivation" => @"Du bist ein enthusiastischer Fitnesstrainer. 

Antworte im folgenden strukturierten Format mit Markdown:

## 💪 MOTIVATION
[Kurze motivierende Einleitung]

## ✨ HEUTE FOKUSSIEREN
• **[Punkt 1]** - [Kurze Erklärung]
• **[Punkt 2]** - [Kurze Erklärung]
• **[Punkt 3]** - [Kurze Erklärung]

## 🎯 ZIEL IM BLICK
[Motivierende Schlussworte mit konkretem Ziel]

Nutze Emojis und formatiere mit Markdown für bessere Lesbarkeit.",

            "fitness" => @"Du bist ein Fitnessexperte, der Trainingsdaten analysiert. 

Erstelle eine strukturierte Analyse im folgenden Markdown-Format:

## 📊 TRAININGSANALYSE
**Gesamtbewertung:** [1-2 Sätze zur allgemeinen Leistung]
**Trainingsvolumen:** [Bewertung der Häufigkeit und Dauer]
**Intensität:** [Bewertung der Trainingsintensität]

## 💡 WICHTIGE ERKENNTNISSE
• **[Erkenntnis 1]** - [Detaillierte Erklärung]
• **[Erkenntnis 2]** - [Detaillierte Erklärung]
• **[Erkenntnis 3]** - [Detaillierte Erklärung]

## 🚀 EMPFEHLUNGEN
1. **Sofort umsetzbar:** [Konkrete Maßnahme für diese Woche]
2. **Mittelfristig:** [Strategische Anpassung für nächsten Monat]
3. **Langfristig:** [Zielorientierte Empfehlung für 3+ Monate]

Verwende präzise Fitness-Terminologie und konkrete Zahlen.",

            "health" => @"Du bist ein Gesundheitsexperte, der Fitnessdaten für Wellness-Erkenntnisse analysiert.

Erstelle eine strukturierte Gesundheitsanalyse im Markdown-Format:

## 🏥 GESUNDHEITSANALYSE
**Belastungsmanagement:** [Bewertung der Trainingsbelastung]
**Regeneration:** [Einschätzung der Erholungsphasen]
**Verletzungsrisiko:** [Risikoeinschätzung basierend auf Daten]

## ⚠️ GESUNDHEITSINDIKATOREN
• **[Indikator 1]** - [Gesundheitliche Bedeutung]
• **[Indikator 2]** - [Gesundheitliche Bedeutung]
• **[Indikator 3]** - [Gesundheitliche Bedeutung]

## 🌱 WELLNESS-EMPFEHLUNGEN
1. **Regeneration:** [Konkrete Erholungsmaßnahmen]
2. **Prävention:** [Verletzungsvorbeugung]
3. **Langfristige Gesundheit:** [Nachhaltige Trainingsansätze]

Fokussiere auf Gesundheit und nachhaltige Trainingsgewohnheiten.",

            "analysis" => @"Du bist ein Sportwissenschaftler, der athletische Leistungsdaten analysiert.

Erstelle eine detaillierte Leistungsanalyse im Markdown-Format:

## 📈 LEISTUNGSANALYSE
**Performance-Trend:** [Entwicklung der Leistung über Zeit]
**Effizienz:** [Verhältnis von Aufwand zu Ergebnis]
**Stärken/Schwächen:** [Identifizierte Leistungsbereiche]

## 🔍 DATENERKENNTNISSE
• **[Metrik 1]** - [Sportwissenschaftliche Interpretation]
• **[Metrik 2]** - [Sportwissenschaftliche Interpretation]
• **[Metrik 3]** - [Sportwissenschaftliche Interpretation]

## ⚡ LEISTUNGSOPTIMIERUNG
1. **Technik:** [Verbesserungen der Ausführung]
2. **Training:** [Anpassungen im Trainingsplan]
3. **Periodisierung:** [Langfristige Planung]

Verwende sportwissenschaftliche Begriffe und quantitative Analysen.",

            _ => @"Du bist ein hilfreicher Fitnessassistent.

Antworte strukturiert im Markdown-Format:

## 📝 ANALYSE
[Hauptanalyse der Situation]

## 💡 ERKENNTNISSE
• **[Punkt 1]** - [Erklärung]
• **[Punkt 2]** - [Erklärung]
• **[Punkt 3]** - [Erklärung]

## 🎯 EMPFEHLUNGEN
1. [Konkrete Maßnahme 1]
2. [Konkrete Maßnahme 2]
3. [Konkrete Maßnahme 3]

Halte die Antwort praktisch und umsetzbar."
        };

        return $"{systemContext}\n\n{originalPrompt}";
    }

    private string GetFallbackResponse(string modelType, string errorType)
    {
        _logger.LogInformation("Generating fallback response for {ModelType} due to: {Error}", modelType, errorType);

        return modelType.ToLower() switch
        {
            "motivation" => $"Bleib dran! Jedes Training bringt dich deinen Zielen näher. Du schaffst das! 💪 (Hinweis: KI-Analyse vorübergehend nicht verfügbar - {errorType})",

            "fitness" => $"Deine Trainingsdaten zeigen konsistente Trainingsmuster und positive Fortschritte. Setze deinen aktuellen Ansatz fort und fokussiere dich auf schrittweise Verbesserung. (Hinweis: Detaillierte Analyse vorübergehend nicht verfügbar - {errorType})",

            "health" => $"Deine Trainingsmuster deuten auf einen gesunden Fitnessansatz hin. Halte weiterhin eine gute Balance zwischen Aktivität und Erholung. (Hinweis: Gesundheitsanalyse vorübergehend nicht verfügbar - {errorType})",

            "analysis" => $"Deine Leistungsdaten zeigen stetige Verbesserung und gute Trainingskonsistenz. Konzentriere dich darauf, dein aktuelles Momentum beizubehalten. (Hinweis: Detaillierte Analyse vorübergehend nicht verfügbar - {errorType})",

            _ => $"Deine Fitnessreise zeigt exzellente Fortschritte! Setze deinen engagierten Ansatz fort und halte die Konsistenz in deiner Trainingsroutine bei. (Hinweis: KI-Analyse vorübergehend nicht verfügbar - {errorType})"
        };
    }
}