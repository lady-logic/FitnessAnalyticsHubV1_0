namespace AIAssistant._02_Application.DTOs;

public class WorkoutAnalysisResponseDto
{
    public string Analysis { get; set; } = string.Empty;
    public List<string>? KeyInsights { get; set; }
    public List<string>? Recommendations { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
