namespace AIAssistant.Application.DTOs;

public class WorkoutAnalysisResponseDto
{
    public string Analysis { get; set; } = string.Empty;
    public List<string>? KeyInsights { get; set; }
    public List<string>? Recommendations { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
