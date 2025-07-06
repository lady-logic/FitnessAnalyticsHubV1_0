using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AIAssistant.Applications.DTOs;

public class MotivationRequestDto
{
    public AthleteProfileDto AthleteProfile { get; set; } = new();
    public WorkoutDataDto? LastWorkout { get; set; }
    public string? UpcomingWorkoutType { get; set; }
    [Required]
    public bool IsStruggling { get; set; }
}
