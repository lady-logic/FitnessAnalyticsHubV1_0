namespace AIAssistant._03_Infrastructure.OpenAI.Models;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public string ApiBaseUrl { get; set; } = "https://api.openai.com/v1/";
}
