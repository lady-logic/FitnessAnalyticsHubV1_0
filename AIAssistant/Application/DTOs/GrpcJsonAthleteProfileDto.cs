namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// AthleteProfile DTO für gRPC-JSON
/// (Entspricht dem gRPC AthleteProfile als JSON)
/// </summary>
public class GrpcJsonAthleteProfileDto
{
    public string? Name { get; set; }
    public string? FitnessLevel { get; set; }
    public string? PrimaryGoal { get; set; }
}
