using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant.UI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotivationCoachController : ControllerBase
{
    private readonly IMotivationCoachService _motivationCoachService;
    private readonly ILogger<MotivationCoachController> _logger;

    public MotivationCoachController(
        IMotivationCoachService motivationCoachService,
        ILogger<MotivationCoachController> logger)
    {
        _motivationCoachService = motivationCoachService;
        _logger = logger;
    }

    [HttpPost("motivate")]
    public async Task<ActionResult<MotivationResponseDto>> GetMotivation(
        [FromBody] MotivationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Generating motivational message for athlete: {Name}",
                request.AthleteProfile.Name);

            // Nutze HuggingFace (alte OpenAI/Claude Methoden sind jetzt redirects)
            var result = await _motivationCoachService.GetHuggingFaceMotivationalMessageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating motivational message");
            return StatusCode(500, "An error occurred while generating motivational message");
        }
    }

    // Neuer spezifischer HuggingFace Endpoint (optional)
    [HttpPost("motivate/huggingface")]
    public async Task<ActionResult<MotivationResponseDto>> GetHuggingFaceMotivation(
        [FromBody] MotivationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Generating HuggingFace motivational message for athlete: {Name}",
                request.AthleteProfile.Name);

            var result = await _motivationCoachService.GetHuggingFaceMotivationalMessageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HuggingFace motivational message");
            return StatusCode(500, "An error occurred while generating motivational message");
        }
    }

    // Health Check für HuggingFace Service
    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            // Einfache Test-Anfrage
            var testRequest = new MotivationRequestDto
            {
                AthleteProfile = new AthleteProfileDto
                {
                    Name = "Test User",
                    FitnessLevel = "Beginner",
                    PrimaryGoal = "Health Check"
                },
                IsStruggling = false
            };

            var result = await _motivationCoachService.GetHuggingFaceMotivationalMessageAsync(testRequest);

            return Ok(new
            {
                status = "healthy",
                message = "HuggingFace service is responding",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}