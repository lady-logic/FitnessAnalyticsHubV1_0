namespace FitnessAnalyticsHub.Application.Interfaces
{
    using FitnessAnalyticsHub.Application.DTOs;

    public interface IAIAssistantClientService
    {
        Task<AIMotivationResponseDto> GetMotivationAsync(AIMotivationRequestDto request, CancellationToken cancellationToken);

        Task<AIWorkoutAnalysisResponseDto> GetWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken);

        Task<AIWorkoutAnalysisResponseDto> GetPerformanceTrendsAsync(int athleteId, CancellationToken cancellationToken, string timeFrame = "month");

        Task<AIWorkoutAnalysisResponseDto> GetTrainingRecommendationsAsync(int athleteId, CancellationToken cancellationToken);

        Task<AIWorkoutAnalysisResponseDto> AnalyzeHealthMetricsAsync(int athleteId, List<AIWorkoutDataDto> recentWorkouts, CancellationToken cancellationToken);

        Task<AIWorkoutAnalysisResponseDto> GetGoogleGeminiWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken);

        Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
    }
}
