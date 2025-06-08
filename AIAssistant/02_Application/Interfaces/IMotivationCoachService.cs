using AIAssistant._02_Application.DTOs;

namespace AIAssistant._02_Application.Interfaces;

public interface IMotivationCoachService
{
    Task<MotivationResponseDto> GetOpenAIMotivationalMessageAsync(
        MotivationRequestDto request);

    Task<MotivationResponseDto> GetClaudeMotivationalMessageAsync(
        MotivationRequestDto request);
}
