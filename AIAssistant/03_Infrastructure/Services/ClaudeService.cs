using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Claude;
using AIAssistant._03_Infrastructure.Claude.Models;
using AIAssistant._03_Infrastructure.OpenAI.Models;

namespace FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Services
{
    //public class ClaudeService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly ClaudeSettings _settings;
    //    private readonly ILogger<ClaudeService> _logger;

    //    public ClaudeService(
    //        HttpClient httpClient,
    //        IOptions<ClaudeSettings> settings,
    //        ILogger<ClaudeService> logger)
    //    {
    //        _httpClient = httpClient;
    //        _settings = settings.Value;
    //        _logger = logger;

    //        // HTTP Client konfigurieren
    //        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
    //        _httpClient.DefaultRequestHeaders.Authorization =
    //            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    //        _httpClient.DefaultRequestHeaders.Accept.Add(
    //            new MediaTypeWithQualityHeaderValue("application/json"));
    //    }

    //    public async Task<string> GetClaudeCompletionAsync(List<Message> messages)
    //    {
    //        try
    //        {
    //            // Convert standard messages to Claude format
    //            var claudeMessages = messages.Select(m => new ClaudeMessage
    //            {
    //                Role = m.Role.ToLower() == "user" ? "user" : "assistant",
    //                Content = new List<ClaudeTextContent>
    //                {
    //            new ClaudeTextContent
    //            {
    //                Type = "text",
    //                Text = m.Content
    //            }
    //                }
    //            }).ToList<object>();  // Cast to List<object> to match Messages property type

    //            var request = new ClaudeRequest
    //            {
    //                Model = _settings.Model,
    //                Messages = claudeMessages,
    //                Temperature = 0.7,
    //            };

    //            var content = new StringContent(
    //                JsonSerializer.Serialize(request),
    //            Encoding.UTF8,
    //                "application/json");

    //            // Anthropic API Endpunkt
    //            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);

    //            // Stelle sicher, dass du im HttpClient den Authorization-Header korrekt setzt:
    //            // _httpClient.DefaultRequestHeaders.Add("x-api-key", "YOUR_ANTHROPIC_API_KEY");
    //            // _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

    //            response.EnsureSuccessStatusCode();
    //            var responseBody = await response.Content.ReadAsStringAsync();

    //            // Claude returns a different response format
    //            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(
    //                responseBody,
    //                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //            return claudeResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting Claude completion");
    //            return "An error occurred while getting Claude completion";
    //        }
    //    }
    //}
}
