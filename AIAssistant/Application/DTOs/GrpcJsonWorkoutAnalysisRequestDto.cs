namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für gRPC-JSON WorkoutAnalysis
/// </summary>
public class GrpcJsonWorkoutAnalysisRequestDto
{
    public GrpcJsonAthleteProfileDto? AthleteProfile { get; set; }

    public GrpcJsonWorkoutDto[]? RecentWorkouts { get; set; }

    public string? AnalysisType { get; set; }

    public string[]? FocusAreas { get; set; }

    public string? PreferredAiProvider { get; set; }
}
