using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für Health Metrics Analysis
/// </summary>
public class GrpcJsonHealthMetricsRequestDto
{
    [Required]
    public int AthleteId { get; set; }
    public GrpcJsonWorkoutDto[]? RecentWorkouts { get; set; }
}
