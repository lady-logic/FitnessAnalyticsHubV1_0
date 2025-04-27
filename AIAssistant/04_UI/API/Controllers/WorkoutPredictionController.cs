using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant._04_UI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutPredictionController : ControllerBase
    {
        private readonly IWorkoutPredictionService _workoutPredictionService;
        private readonly ILogger<WorkoutPredictionController> _logger;

        public WorkoutPredictionController(
            IWorkoutPredictionService workoutPredictionService,
            ILogger<WorkoutPredictionController> logger)
        {
            _workoutPredictionService = workoutPredictionService;
            _logger = logger;
        }

        [HttpPost("predict")]
        public async Task<ActionResult<WorkoutPredictionResponseDto>> PredictOpenAIWorkout(
            [FromBody] WorkoutPredictionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Predicting workout performance for type: {Type}",
                    request.TargetWorkoutType);

                var result = await _workoutPredictionService.PredictOpenAIWorkoutPerformanceAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting workout performance");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("predict")]
        public async Task<ActionResult<WorkoutPredictionResponseDto>> PredictClaudeWorkout(
            [FromBody] WorkoutPredictionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Predicting workout performance for type: {Type}",
                    request.TargetWorkoutType);

                var result = await _workoutPredictionService.PredictClaudeWorkoutPerformanceAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting workout performance");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
