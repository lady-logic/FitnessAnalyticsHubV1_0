using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpGet("performance/{athleteId}/{sportType}")]
    public async Task<ActionResult<PredictionResultDto>> PredictPerformance(int athleteId, string sportType, CancellationToken cancellationToken)
    {
        try
        {
            var prediction = await _predictionService.PredictPerformanceAsync(athleteId, sportType, cancellationToken);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("train/{athleteId}")]
    public async Task<IActionResult> TrainModel(int athleteId)
    {
        try
        {
            await _predictionService.TrainModelAsync(athleteId);
            return Ok("Modell erfolgreich trainiert.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("is-model-trained/{athleteId}")]
    public async Task<ActionResult<bool>> IsModelTrained(int athleteId, CancellationToken cancellationToken)
    {
        try
        {
            var isTrained = await _predictionService.IsModelTrainedForAthleteAsync(athleteId, cancellationToken);
            return Ok(isTrained);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("evaluate/{athleteId}")]
    public async Task<ActionResult<ModelEvaluationDto>> EvaluateModel(int athleteId)
    {
        try
        {
            var evaluation = await _predictionService.EvaluateModelAsync(athleteId);
            return Ok(evaluation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
