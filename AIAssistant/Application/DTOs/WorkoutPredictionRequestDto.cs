using FitnessAnalyticsHub.AIAssistant.Application.DTOs;

namespace AIAssistant.Application.DTOs;

public class WorkoutPredictionRequestDto
{
    public List<WorkoutDataDto> PastWorkouts { get; set; } = new();

    public string TargetWorkoutType { get; set; } = string.Empty;

    public DateTime TargetDate { get; set; }

    public string? Goal { get; set; } // "Improve Speed", "Increase Distance", etc.
}
