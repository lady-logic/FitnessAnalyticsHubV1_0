using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIAssistantClientService _aiAssistant;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IAIAssistantClientService aiAssistant,
        ILogger<AIController> logger)
    {
        _aiAssistant = aiAssistant;
        _logger = logger;
    }

    [HttpPost("motivation")]
    public async Task<ActionResult<AIMotivationResponseDto>> GetMotivation(
        AIMotivationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("AI motivation request for athlete: {AthleteName}",
                request.AthleteProfile?.Name ?? "Unknown");

            var result = await _aiAssistant.GetMotivationAsync(request, cancellationToken);

            _logger.LogInformation("AI motivation response generated from source: {Source}", result.Source);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI motivation request");
            return StatusCode(500, "An error occurred while generating motivation");
        }
    }

    [HttpPost("analysis")]
    public async Task<ActionResult<AIWorkoutAnalysisResponseDto>> GetWorkoutAnalysis(
        AIWorkoutAnalysisRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("AI workout analysis request for {WorkoutCount} workouts, type: {AnalysisType}",
                request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

            var result = await _aiAssistant.GetWorkoutAnalysisAsync(request, cancellationToken);

            _logger.LogInformation("AI analysis response generated from source: {Source}", result.Source);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI workout analysis request");
            return StatusCode(500, "An error occurred while generating workout analysis");
        }
    }

    [HttpGet("health")]
    public async Task<ActionResult<object>> GetAIHealth(CancellationToken cancellationToken)
    {
        try
        {
            var isHealthy = await _aiAssistant.IsHealthyAsync(cancellationToken);

            return Ok(new
            {
                isHealthy = isHealthy,
                service = "AIAssistant",
                timestamp = DateTime.UtcNow,
                status = isHealthy ? "Available" : "Unavailable"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI health check failed");
            return Ok(new
            {
                isHealthy = false,
                service = "AIAssistant",
                timestamp = DateTime.UtcNow,
                status = "Unavailable",
                error = ex.Message
            });
        }
    }
}