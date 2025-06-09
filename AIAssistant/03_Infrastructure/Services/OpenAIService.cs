namespace AIAssistant._03_Infrastructure.Services;

//public class OpenAIService : IAIPromptService
//{
//    private readonly HttpClient _httpClient;
//    private readonly OpenAISettings _settings;
//    private readonly ILogger<OpenAIService> _logger;

//    public OpenAIService(
//        HttpClient httpClient,
//        IOptions<OpenAISettings> settings,
//        ILogger<OpenAIService> logger)
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

//    public async Task<string> GetCompletionAsync(string prompt)
//    {
//        var messages = new List<Message>
//        {
//            new Message { Role = "system", Content = "You are a fitness expert assistant." },
//            new Message { Role = "user", Content = prompt }
//        };

//        return await GetChatCompletionAsync(messages);
//    }

//    public async Task<string> GetChatCompletionAsync(List<Message> messages)
//    {
//        try
//        {
//            var request = new OpenAIRequest
//            {
//                Model = _settings.Model,
//                Messages = messages,
//                Temperature = 0.7,
//                MaxTokens = 500
//            };
//            var content = new StringContent(
//                JsonSerializer.Serialize(request),
//                Encoding.UTF8,
//                "application/json");
//            var response = await _httpClient.PostAsync("chat/completions", content);
//            response.EnsureSuccessStatusCode();
//            var responseBody = await response.Content.ReadAsStringAsync();
//            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(
//                responseBody,
//                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
//            return openAIResponse?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error predicting workout performance");
//            return "An error occurred while predicting workout performance";
//        }
//    }
//}
