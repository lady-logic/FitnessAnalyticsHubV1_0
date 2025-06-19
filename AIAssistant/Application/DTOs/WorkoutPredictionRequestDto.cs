namespace AIAssistant._02_Application.DTOs;

public class WorkoutPredictionRequestDto
{
    public List<Domain.Models.WorkoutData> PastWorkouts { get; set; } = new();
    public string TargetWorkoutType { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string? Goal { get; set; } // "Improve Speed", "Increase Distance", etc.
}
