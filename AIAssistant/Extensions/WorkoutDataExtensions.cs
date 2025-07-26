using AIAssistant.Application.DTOs;
using AIAssistant.Domain.Models;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;

namespace FitnessAnalyticsHub.AIAssistant.Extensions;

public static class WorkoutDataExtensions
{
    public static WorkoutData ToDomain(this WorkoutDataDto dto)
    {
        return new WorkoutData
        {
            Date = dto.Date,
            ActivityType = dto.ActivityType,
            Distance = dto.Distance,
            Duration = dto.Duration,
            Calories = dto.Calories,
            MetricsData = dto.MetricsData,
        };
    }

    public static WorkoutDataDto ToDto(this WorkoutData domain)
    {
        return new WorkoutDataDto
        {
            Date = domain.Date,
            ActivityType = domain.ActivityType,
            Distance = domain.Distance,
            Duration = domain.Duration,
            Calories = domain.Calories,
            MetricsData = domain.MetricsData,
        };
    }

    public static WorkoutDataDto ToWorkoutDataDto(this global::Fitnessanalyticshub.Workout grpcWorkout)
    {
        return new WorkoutDataDto
        {
            Date = DateTime.TryParse(grpcWorkout.Date, out var date) ? date : DateTime.UtcNow,
            ActivityType = grpcWorkout.ActivityType ?? string.Empty,
            Distance = grpcWorkout.Distance,
            Duration = grpcWorkout.Duration,
            Calories = grpcWorkout.Calories,
            MetricsData = null,
        };
    }

    public static WorkoutDataDto ToWorkoutDataDto(this GrpcJsonWorkoutDto grpcWorkout)
    {
        return new WorkoutDataDto
        {
            Date = DateTime.TryParse(grpcWorkout.Date, out var date) ? date : DateTime.UtcNow,
            ActivityType = grpcWorkout.ActivityType ?? string.Empty,
            Distance = grpcWorkout.Distance,
            Duration = grpcWorkout.Duration,
            Calories = grpcWorkout.Calories,
        };
    }

    public static WorkoutAnalysisRequestDto ToWorkoutAnalysisRequestDto(
    this GrpcJsonWorkoutAnalysisRequestDto grpcJsonRequest)
    {
        return new WorkoutAnalysisRequestDto
        {
            AthleteProfile = grpcJsonRequest.AthleteProfile?.ToAthleteProfileDto(),
            RecentWorkouts = grpcJsonRequest.RecentWorkouts?.Select(w => w.ToWorkoutDataDto()).ToList()
                            ?? new List<WorkoutDataDto>(),
            AnalysisType = grpcJsonRequest.AnalysisType ?? "Performance",
        };
    }
}
