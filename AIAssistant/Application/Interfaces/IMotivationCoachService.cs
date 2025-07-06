using AIAssistant.Application.DTOs;
using AIAssistant.Applications.DTOs;

namespace AIAssistant.Application.Interfaces;

public interface IMotivationCoachService
{
    Task<MotivationResponseDto> GetHuggingFaceMotivationalMessageAsync(
        MotivationRequestDto request);
}
