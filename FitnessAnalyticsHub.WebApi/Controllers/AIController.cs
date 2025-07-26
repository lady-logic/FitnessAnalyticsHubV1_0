using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIAssistantClientService aiAssistant;
    private readonly ILogger<AIController> logger;

    public AIController(
        IAIAssistantClientService aiAssistant,
        ILogger<AIController> logger)
    {
        this.aiAssistant = aiAssistant;
        this.logger = logger;
    }

    [HttpPost("motivation")]
    public async Task<ActionResult<AIMotivationResponseDto>> GetMotivation(
        AIMotivationRequestDto request,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "AI motivation request for athlete: {AthleteName}",
            request.AthleteProfile?.Name ?? "Unknown");

        AIMotivationResponseDto result = await this.aiAssistant.GetMotivationAsync(request, cancellationToken);

        this.logger.LogInformation("AI motivation response generated from source: {Source}", result.Source);

        return this.Ok(result);
    }

    [HttpPost("analysis")]
    public async Task<ActionResult<AIWorkoutAnalysisResponseDto>> GetWorkoutAnalysis(
        AIWorkoutAnalysisRequestDto request,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "AI workout analysis request for {WorkoutCount} workouts, type: {AnalysisType}",
            request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

        AIWorkoutAnalysisResponseDto result = await this.aiAssistant.GetWorkoutAnalysisAsync(request, cancellationToken);

        this.logger.LogInformation("AI analysis response generated from source: {Source}", result.Source);

        return this.Ok(result);
    }

    [HttpGet("health")]
    public async Task<ActionResult<object>> GetAIHealth(CancellationToken cancellationToken)
    {
        bool isHealthy = await this.aiAssistant.IsHealthyAsync(cancellationToken);

        return this.Ok(new
        {
            isHealthy = isHealthy,
            service = "AIAssistant",
            timestamp = DateTime.UtcNow,
            status = isHealthy ? "Available" : "Unavailable",
        });
    }
}