namespace FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Claude;

public class ClaudeMessage
{
    public string Role { get; set; }
    public List<ClaudeTextContent> Content { get; set; }
}
