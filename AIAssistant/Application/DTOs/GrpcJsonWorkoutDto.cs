namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Workout DTO für gRPC-JSON
/// </summary>
public class GrpcJsonWorkoutDto
{
    public string Date { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public double Distance { get; set; }
    public int Duration { get; set; }
    public int Calories { get; set; }
}
