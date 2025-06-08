using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant._04_UI.API.Controllers;

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
    public async Task<ActionResult<MotivationResponseDto>> GetOpenAIMotivation(
        [FromBody] MotivationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Generating motivational message for athlete: {Name}",
                request.AthleteProfile.Name);

            var result = await _motivationCoachService.GetOpenAIMotivationalMessageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating motivational message");
            return StatusCode(500, "An error occurred while generating motivational message");
        }
    }

    [HttpPost("motivate")]
    public async Task<ActionResult<MotivationResponseDto>> GetClaudeMotivation(
        [FromBody] MotivationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Generating motivational message for athlete: {Name}",
                request.AthleteProfile.Name);

            var result = await _motivationCoachService.GetClaudeMotivationalMessageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating motivational message");
            return StatusCode(500, "An error occurred while generating motivational message");
        }
    }
}
