using AIAssistant._03_Infrastructure.OpenAI.Models;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Claude;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Common;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.OpenAI.Models;
using AIAssistant._02_Application.Interfaces;

namespace FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Services
{
    public class LLMService : IAIPromptService
    {
        private readonly HttpClient _openAIClient;
        private readonly HttpClient _claudeClient;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;

        public LLMService(
            HttpClient openAIClient,
            HttpClient claudeClient,
            ILogger<LLMService> logger,
            IOptions<AppSettings> settings)
        {
            _openAIClient = openAIClient;
            _claudeClient = claudeClient;
            _logger = logger;
            _settings = settings.Value;

            // OpenAI API Headers
            _openAIClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OpenAIApiKey}");

            // Claude API Headers
            _claudeClient.DefaultRequestHeaders.Add("x-api-key", _settings.ClaudeApiKey);
            _claudeClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        // OpenAI Completion
        public async Task<string> GetOpenAICompletionAsync(List<Message> messages)
        {
            try
            {
                var request = new OpenAIRequest
                {
                    Model = _settings.OpenAIModel,
                    Messages = messages,
                    Temperature = 0.7,
                    MaxTokens = 500
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _openAIClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return openAIResponse?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenAI completion");
                return "An error occurred while getting OpenAI completion";
            }
        }

        // Claude Completion
        public async Task<string> GetClaudeCompletionAsync(List<Message> messages)
        {
            try
            {
                // Convert standard messages to Claude format
                var claudeMessages = messages.Select(m => new ClaudeMessage
                {
                    Role = m.Role.ToLower() == "user" ? "user" : "assistant",
                    Content = new List<ClaudeTextContent>
                {
                    new ClaudeTextContent
                    {
                        Type = "text",
                        Text = m.Content
                    }
                }
                }).ToList<object>();

                var request = new ClaudeRequest
                {
                    Model = _settings.ClaudeModel ?? "claude-3-7-sonnet-20250219",
                    Messages = claudeMessages,
                    Temperature = 0.7,
                    Max_tokens = 500 // Beachte den Unterschied: snake_case
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _claudeClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return claudeResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Claude completion");
                return "An error occurred while getting Claude completion";
            }
        }

        public async Task<string> GetOpenAICompletionAsync(string prompt)
        {
            var messages = new List<Message>
                {
                    new Message { Role = "system", Content = "You are a fitness expert assistant." },
                    new Message { Role = "user", Content = prompt }
                };

            return await GetOpenAICompletionAsync(messages);
        }

        public async Task<string> GetClaudeCompletionAsync(string prompt)
        {
            var messages = new List<Message>
            {
                new Message { Role = "system", Content = "You are a fitness expert assistant." },
                new Message { Role = "user", Content = prompt }
            };

            return await GetClaudeCompletionAsync(messages);
        }
    }
}
