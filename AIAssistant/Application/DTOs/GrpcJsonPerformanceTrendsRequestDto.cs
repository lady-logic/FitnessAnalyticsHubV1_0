namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO für Performance Trends
/// </summary>
public class GrpcJsonPerformanceTrendsRequestDto
{
    [Required]
    public int AthleteId { get; set; }
    public string TimeFrame { get; set; } = "month";
}
