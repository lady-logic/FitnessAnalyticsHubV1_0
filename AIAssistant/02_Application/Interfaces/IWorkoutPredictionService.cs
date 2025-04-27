using AIAssistant._02_Application.DTOs;

namespace AIAssistant._02_Application.Interfaces
{
    public interface IWorkoutPredictionService
    {
        Task<WorkoutPredictionResponseDto> PredictOpenAIWorkoutPerformanceAsync(
            WorkoutPredictionRequestDto request);

        Task<WorkoutPredictionResponseDto> PredictClaudeWorkoutPerformanceAsync(
            WorkoutPredictionRequestDto request);
    }
}
