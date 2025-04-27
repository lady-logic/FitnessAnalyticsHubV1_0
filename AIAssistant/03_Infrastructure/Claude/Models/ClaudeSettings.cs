namespace AIAssistant._03_Infrastructure.Claude.Models
{
    public class ClaudeSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "claude-3-7-sonnet-20250219";
        public string ApiBaseUrl { get; set; } = "https://api.openai.com/v1/";
    }
}
