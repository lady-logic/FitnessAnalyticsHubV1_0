using System.Text;
using System.Text.Json;
using AIAssistant.Application.Interfaces;
using AIAssistant.Infrastructure.OpenAI.Models;

namespace FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;

public class HuggingFaceService : IAIPromptService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuggingFaceService> _logger;
    private readonly string? _apiKey;

    // Verschiedene Models für verschiedene Zwecke (WORKING 2025 Provider!)
    private readonly Dictionary<string, (string provider, string model, string url)> _models = new()
    {
        { "fitness", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "health", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "motivation", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "analysis", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") }
    };

    public HuggingFaceService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HuggingFaceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // HuggingFace API Key aus Configuration (versuche beide Namen)
        _apiKey = configuration["HuggingFace:ApiKey"] ??
                 configuration["HuggingFace:ApiToken"];

        _logger.LogInformation("HuggingFaceService initializing...");
        _logger.LogInformation("Tried ApiKey: {HasApiKey}", !string.IsNullOrEmpty(configuration["HuggingFace:ApiKey"]));
        _logger.LogInformation("Tried ApiToken: {HasApiToken}", !string.IsNullOrEmpty(configuration["HuggingFace:ApiToken"]));
        _logger.LogInformation("Final API Key present: {HasApiKey}", !string.IsNullOrEmpty(_apiKey));
        _logger.LogInformation("API Key length: {KeyLength}", _apiKey?.Length ?? 0);

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _logger.LogInformation("HuggingFace API Key configured for Inference Providers");
        }
        else
        {
            _logger.LogWarning("No HuggingFace API Key - service will use fallbacks");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger.LogInformation("HuggingFaceService initialized successfully");
    }


    public async Task<string> GetFitnessAnalysisAsync(string prompt)
    {
        return await GetHuggingFaceCompletionAsync(prompt, "fitness");
    }

    public async Task<string> GetHealthAnalysisAsync(string prompt)
    {
        return await GetHuggingFaceCompletionAsync(prompt, "health");
    }

    public async Task<string> GetMotivationAsync(string prompt)
    {
        return await GetHuggingFaceCompletionAsync(prompt, "motivation");
    }

    private async Task<string> GetHuggingFaceCompletionAsync(string prompt, string modelType)
    {
        // Erstelle erweiterten Prompt basierend auf Kontext
        var enhancedPrompt = CreateEnhancedPrompt(prompt, modelType);

        try
        {
            var modelConfig = _models.GetValueOrDefault(modelType, _models["motivation"]);
            var (provider, model, url) = modelConfig;

            _logger.LogInformation("Calling HuggingFace Inference Provider: {Provider} with model: {Model} for type: {ModelType}",
                provider, model, modelType);

            // Nutze Chat Completions API (OpenAI-kompatibel)
            var requestPayload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = enhancedPrompt }
                },
                max_tokens = 150,
                temperature = 0.7,
                stream = false
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Request URL: {Url}", url);
            _logger.LogDebug("Request payload: {Payload}", jsonContent);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("Response content: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("HuggingFace API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                _logger.LogError("Request URL: {Url}", url);
                _logger.LogError("Request payload: {Payload}", jsonContent);
                _logger.LogError("API Key used: {ApiKeyPreview}...", _apiKey?.Substring(0, Math.Min(10, _apiKey.Length)));

                // Spezifische Fehlerbehandlung
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("UNAUTHORIZED: Check your HuggingFace API key permissions!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("RATE LIMITED: HuggingFace rate limit exceeded");
                }

                // Fallback response für verschiedene Fehler
                return GetFallbackResponse(modelType, response.StatusCode.ToString());
            }

            // Parse OpenAI-kompatible Response
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseJson.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var messageContent))
                {
                    var result = messageContent.GetString() ?? string.Empty;
                    _logger.LogInformation("Successfully received response from HuggingFace Inference Provider");
                    return result.Trim();
                }
            }

            _logger.LogWarning("Unexpected response format from HuggingFace Inference Provider");
            return GetFallbackResponse(modelType, "unexpected_format");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("HuggingFace request timeout for model type: {ModelType}", modelType);
            return GetFallbackResponse(modelType, "timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling HuggingFace Inference Provider for {ModelType}", modelType);
            return GetFallbackResponse(modelType, ex.Message);
        }
    }

    private string CreateEnhancedPrompt(string originalPrompt, string modelType)
    {
        var systemContext = modelType.ToLower() switch
        {
            "motivation" => "You are an enthusiastic fitness coach. Provide motivational and encouraging response.",
            "fitness" => "You are a fitness expert analyzing workout data. Provide professional insights.",
            "health" => "You are a health professional analyzing fitness data for wellness insights.",
            "analysis" => "You are a sports scientist analyzing athletic performance data.",
            _ => "You are a helpful fitness assistant."
        };

        return $"{systemContext}\n\n{originalPrompt}";
    }

    private string ConvertMessagesToPrompt(List<Message> messages)
    {
        var promptBuilder = new StringBuilder();

        foreach (var message in messages)
        {
            var role = message.Role.ToLower() switch
            {
                "system" => "System",
                "user" => "User",
                "assistant" => "Assistant",
                _ => "User"
            };

            promptBuilder.AppendLine($"{role}: {message.Content}");
        }

        return promptBuilder.ToString();
    }

    private string GetFallbackResponse(string modelType, string errorType)
    {
        _logger.LogInformation("Generating fallback response for {ModelType} due to: {Error}", modelType, errorType);

        return modelType.ToLower() switch
        {
            "motivation" => $"Keep pushing forward! Every workout brings you closer to your goals. You've got this! 💪 (Note: AI analysis temporarily unavailable - {errorType})",

            "fitness" => $"Your workout data shows consistent training patterns and positive progression. Continue with your current approach while focusing on gradual improvement. (Note: Detailed analysis temporarily unavailable - {errorType})",

            "health" => $"Your training patterns indicate a healthy approach to fitness. Continue maintaining good balance between activity and recovery. (Note: Health analysis temporarily unavailable - {errorType})",

            "analysis" => $"Your performance data suggests steady improvement and good training consistency. Focus on maintaining your current momentum. (Note: Detailed analysis temporarily unavailable - {errorType})",

            _ => $"Your fitness journey shows excellent progress! Continue with your dedicated approach and maintain consistency in your training routine. (Note: AI analysis temporarily unavailable - {errorType})"
        };
    }
}