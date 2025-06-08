namespace FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Claude;

public class ClaudeResponse
{
    public string Id { get; set; }
    public string Model { get; set; }
    public List<ClaudeContent> Content { get; set; }
}
