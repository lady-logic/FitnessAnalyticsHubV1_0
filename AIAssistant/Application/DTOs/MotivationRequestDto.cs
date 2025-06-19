namespace AIAssistant._02_Application.DTOs;

public class MotivationRequestDto
{
    public Domain.Models.AthleteProfile AthleteProfile { get; set; } = new();
    public Domain.Models.WorkoutData? LastWorkout { get; set; }
    public string? UpcomingWorkoutType { get; set; }
    public bool IsStruggling { get; set; }
}
