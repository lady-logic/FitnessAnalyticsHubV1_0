namespace AIAssistant.Application.Interfaces;

using AIAssistant.Application.DTOs;
using AIAssistant.Applications.DTOs;

public interface IMotivationCoachService
{
    Task<MotivationResponseDto> GetHuggingFaceMotivationalMessageAsync(
        MotivationRequestDto request, CancellationToken cancellationToken);
}
