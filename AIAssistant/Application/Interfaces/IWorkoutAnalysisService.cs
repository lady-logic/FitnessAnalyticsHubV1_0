namespace AIAssistant.Application.Interfaces;

using AIAssistant.Application.DTOs;

public interface IWorkoutAnalysisService
{
    Task<WorkoutAnalysisResponseDto> AnalyzeHuggingFaceWorkoutsAsync(
        WorkoutAnalysisRequestDto request, CancellationToken cancellationToken);

    Task<WorkoutAnalysisResponseDto> AnalyzeGoogleGeminiWorkoutsAsync(
        WorkoutAnalysisRequestDto request, CancellationToken cancellationToken);
}
