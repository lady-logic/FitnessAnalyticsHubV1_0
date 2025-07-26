namespace FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;

using global::AIAssistant.Application.Interfaces;
using global::AIAssistant.Infrastructure.OpenAI.Models;
using System.Text;
using System.Text.Json;

public class HuggingFaceService : IAIPromptService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<HuggingFaceService> logger;
    private readonly string? apiKey;

    // Verschiedene Models für verschiedene Zwecke (WORKING 2025 Provider!)
    private readonly Dictionary<string, (string provider, string model, string url)> models = new()
    {
        { "fitness", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "health", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "motivation", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
        { "analysis", ("sambanova", "Meta-Llama-3.1-8B-Instruct", "https://router.huggingface.co/sambanova/v1/chat/completions") },
    };

    public HuggingFaceService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HuggingFaceService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;

        // HuggingFace API Key aus Configuration (versuche beide Namen)
        this.apiKey = configuration["HuggingFace:ApiKey"] ??
                 configuration["HuggingFace:ApiToken"];

        this.logger.LogInformation("HuggingFaceService initializing...");
        this.logger.LogInformation("Tried ApiKey: {HasApiKey}", !string.IsNullOrEmpty(configuration["HuggingFace:ApiKey"]));
        this.logger.LogInformation("Tried ApiToken: {HasApiToken}", !string.IsNullOrEmpty(configuration["HuggingFace:ApiToken"]));
        this.logger.LogInformation("Final API Key present: {HasApiKey}", !string.IsNullOrEmpty(this.apiKey));
        this.logger.LogInformation("API Key length: {KeyLength}", this.apiKey?.Length ?? 0);

        if (!string.IsNullOrEmpty(this.apiKey))
        {
            this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.apiKey}");
            this.logger.LogInformation("HuggingFace API Key configured for Inference Providers");
        }
        else
        {
            this.logger.LogWarning("No HuggingFace API Key - service will use fallbacks");
        }

        this.httpClient.Timeout = TimeSpan.FromSeconds(30);
        this.logger.LogInformation("HuggingFaceService initialized successfully");
    }

    public async Task<string> GetFitnessAnalysisAsync(string prompt)
    {
        return await this.GetHuggingFaceCompletionAsync(prompt, "fitness");
    }

    public async Task<string> GetHealthAnalysisAsync(string prompt)
    {
        return await this.GetHuggingFaceCompletionAsync(prompt, "health");
    }

    public async Task<string> GetMotivationAsync(string prompt)
    {
        return await this.GetHuggingFaceCompletionAsync(prompt, "motivation");
    }

    private async Task<string> GetHuggingFaceCompletionAsync(string prompt, string modelType)
    {
        // Erstelle erweiterten Prompt basierend auf Kontext
        var enhancedPrompt = this.CreateEnhancedPrompt(prompt, modelType);

        try
        {
            var modelConfig = this.models.GetValueOrDefault(modelType, this.models["motivation"]);
            var (provider, model, url) = modelConfig;

            this.logger.LogInformation(
                "Calling HuggingFace Inference Provider: {Provider} with model: {Model} for type: {ModelType}",
                provider, model, modelType);

            // Nutze Chat Completions API (OpenAI-kompatibel)
            var requestPayload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = enhancedPrompt },
                },
                max_tokens = 150,
                temperature = 0.7,
                stream = false,
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            this.logger.LogDebug("Request URL: {Url}", url);
            this.logger.LogDebug("Request payload: {Payload}", jsonContent);

            var response = await this.httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            this.logger.LogDebug("Response status: {StatusCode}", response.StatusCode);
            this.logger.LogDebug("Response content: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                this.logger.LogError("HuggingFace API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                this.logger.LogError("Request URL: {Url}", url);
                this.logger.LogError("Request payload: {Payload}", jsonContent);
                this.logger.LogError("API Key used: {ApiKeyPreview}...", this.apiKey?.Substring(0, Math.Min(10, this.apiKey.Length)));

                // Spezifische Fehlerbehandlung
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    this.logger.LogError("UNAUTHORIZED: Check your HuggingFace API key permissions!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    this.logger.LogWarning("RATE LIMITED: HuggingFace rate limit exceeded");
                }

                // Fallback response für verschiedene Fehler
                return this.GetFallbackResponse(modelType, response.StatusCode.ToString());
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
                    this.logger.LogInformation("Successfully received response from HuggingFace Inference Provider");
                    return result.Trim();
                }
            }

            this.logger.LogWarning("Unexpected response format from HuggingFace Inference Provider");
            return this.GetFallbackResponse(modelType, "unexpected_format");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            this.logger.LogWarning("HuggingFace request timeout for model type: {ModelType}", modelType);
            return this.GetFallbackResponse(modelType, "timeout");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error calling HuggingFace Inference Provider for {ModelType}", modelType);
            return this.GetFallbackResponse(modelType, ex.Message);
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
        this.logger.LogInformation("Generating fallback response for {ModelType} due to: {Error}", modelType, errorType);

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