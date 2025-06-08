using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;
using AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant._04_UI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutAnalysisController : ControllerBase
{
    private readonly IWorkoutAnalysisService _workoutAnalysisService;
    private readonly ILogger<WorkoutAnalysisController> _logger;

    public WorkoutAnalysisController(
        IWorkoutAnalysisService workoutAnalysisService,
        ILogger<WorkoutAnalysisController> logger)
    {
        _workoutAnalysisService = workoutAnalysisService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeOpenAIWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await _workoutAnalysisService.AnalyzeOpenAIWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts");
            return StatusCode(500, "An error occurred while analyzing workouts");
        }
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeClaudeWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await _workoutAnalysisService.AnalyzeClaudeWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts");
            return StatusCode(500, "An error occurred while analyzing workouts");
        }
    }
}
