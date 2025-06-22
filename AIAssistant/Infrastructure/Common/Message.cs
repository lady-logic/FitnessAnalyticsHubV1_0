namespace AIAssistant.Infrastructure.OpenAI.Models;

public class Message
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
