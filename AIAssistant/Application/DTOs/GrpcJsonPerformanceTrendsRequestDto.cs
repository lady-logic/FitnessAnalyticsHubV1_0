namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für Performance Trends
/// </summary>
public class GrpcJsonPerformanceTrendsRequestDto
{
    public int AthleteId { get; set; }
    public string TimeFrame { get; set; } = "month";
}
