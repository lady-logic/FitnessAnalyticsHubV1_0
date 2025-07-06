using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für Performance Trends
/// </summary>
public class GrpcJsonPerformanceTrendsRequestDto
{
    [Required]
    public int AthleteId { get; set; }
    public string TimeFrame { get; set; } = "month";
}
