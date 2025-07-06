using AIAssistant.Application.DTOs;
using AIAssistant.Applications.DTOs;
using AIAssistant.Domain.Models;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant.Extensions;

namespace AIAssistant.Extensions;

public static class GrpcMappingExtensions
{
    public static AIAssistant.Domain.Models.AthleteProfile ToAIAssistantAthleteProfile(
        this global::Fitnessanalyticshub.AthleteProfile grpcProfile)
    {
        return new AIAssistant.Domain.Models.AthleteProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = grpcProfile?.Name ?? "",
            FitnessLevel = grpcProfile?.FitnessLevel ?? "",
            PrimaryGoal = grpcProfile?.PrimaryGoal ?? "",
            Preferences = new Dictionary<string, object>()
        };
    }

    public static MotivationRequestDto ToMotivationRequestDto(
        this global::Fitnessanalyticshub.MotivationRequest grpcRequest)
    {
        return new MotivationRequestDto
        {
            AthleteProfile = grpcRequest.AthleteProfile.ToAthleteProfileDto(),
            LastWorkout = null, // Erstmal null - wird nicht über gRPC übertragen
            UpcomingWorkoutType = null, // Erstmal null
            IsStruggling = false // Default
        };
    }

    public static WorkoutAnalysisRequestDto ToWorkoutAnalysisRequestDto(
    this global::Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest)
    {
        return new WorkoutAnalysisRequestDto
        {
            AnalysisType = grpcRequest.AnalysisType ?? "General",
            RecentWorkouts = grpcRequest.RecentWorkouts
                .Select(w => w.ToWorkoutDataDto())
                .ToList(),
            AthleteProfile = grpcRequest.AthleteProfile?.ToAthleteProfileDto() ?? new AthleteProfileDto(),
            AdditionalContext = new Dictionary<string, object>()
        };
    }

    public static WorkoutData ToAIAssistantWorkoutData(
    this global::Fitnessanalyticshub.Workout grpcWorkout)
    {
        return new WorkoutData
        {
            Date = DateTime.Parse(grpcWorkout.Date),
            ActivityType = grpcWorkout.ActivityType,
            Distance = grpcWorkout.Distance,
            Duration = (int)grpcWorkout.Duration,
            Calories = (int)grpcWorkout.Calories,
        };
    }
}