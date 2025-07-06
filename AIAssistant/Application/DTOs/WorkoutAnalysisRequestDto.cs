using FitnessAnalyticsHub.AIAssistant.Application.DTOs;

namespace AIAssistant.Application.DTOs;

public class WorkoutAnalysisRequestDto
{
    public List<WorkoutDataDto> RecentWorkouts { get; set; } = new();
    public string AnalysisType { get; set; } = string.Empty; // "Performance", "Progress", "Recommendations"
    public AthleteProfileDto? AthleteProfile { get; set; }
    public Dictionary<string, object>? AdditionalContext { get; set; }
}
