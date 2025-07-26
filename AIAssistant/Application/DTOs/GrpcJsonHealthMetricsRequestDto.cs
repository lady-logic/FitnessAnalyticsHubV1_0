namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO für Health Metrics Analysis
/// </summary>
public class GrpcJsonHealthMetricsRequestDto
{
    [Required]
    public int AthleteId { get; set; }

    public GrpcJsonWorkoutDto[]? RecentWorkouts { get; set; }
}
