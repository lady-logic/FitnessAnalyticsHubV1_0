namespace AIAssistant.Application.DTOs;

public class WorkoutAnalysisRequestDto
{
    public List<Domain.Models.WorkoutData> RecentWorkouts { get; set; } = new();
    public string AnalysisType { get; set; } = string.Empty; // "Performance", "Progress", "Recommendations"
    public Domain.Models.AthleteProfile? AthleteProfile { get; set; }
    public Dictionary<string, object>? AdditionalContext { get; set; }
}
