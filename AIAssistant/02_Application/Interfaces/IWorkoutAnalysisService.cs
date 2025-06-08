using AIAssistant._02_Application.DTOs;
using AIAssistant.Application.DTOs;

namespace AIAssistant._02_Application.Interfaces;

public interface IWorkoutAnalysisService
{
    Task<WorkoutAnalysisResponseDto> AnalyzeOpenAIWorkoutsAsync(
        WorkoutAnalysisRequestDto request);

    Task<WorkoutAnalysisResponseDto> AnalyzeClaudeWorkoutsAsync(
        WorkoutAnalysisRequestDto request);
}
