namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für gRPC-JSON MotivationService
/// (Entspricht dem gRPC MotivationRequest als JSON)
/// </summary>
public class GrpcJsonMotivationRequestDto
{
    public GrpcJsonAthleteProfileDto? AthleteProfile { get; set; }
    public bool IsStruggling { get; set; } = false;
    public string? UpcomingWorkoutType { get; set; }
}
