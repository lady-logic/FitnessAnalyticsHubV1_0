using FitnessAnalyticsHub.Application;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingPlanController : ControllerBase
{
    private readonly ITrainingPlanService _trainingPlanService;

    public TrainingPlanController(ITrainingPlanService trainingPlanService)
    {
        _trainingPlanService = trainingPlanService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingPlanDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanService.GetTrainingPlanByIdAsync(id, cancellationToken);
        if (trainingPlan == null)
            return NotFound($"Trainingsplan mit ID {id} wurde nicht gefunden.");
        return Ok(trainingPlan);
    }

    [HttpGet("athlete/{athleteId}")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetByAthleteId(int athleteId, CancellationToken cancellationToken)
    {
        var trainingPlans = await _trainingPlanService.GetTrainingPlansByAthleteIdAsync(athleteId, cancellationToken);
        return Ok(trainingPlans);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingPlanDto>> Create(CreateTrainingPlanDto createTrainingPlanDto, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanService.CreateTrainingPlanAsync(createTrainingPlanDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = trainingPlan.Id }, trainingPlan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTrainingPlanDto updateTrainingPlanDto, CancellationToken cancellationToken)
    {
        if (id != updateTrainingPlanDto.Id)
            return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

        try
        {
            await _trainingPlanService.UpdateTrainingPlanAsync(updateTrainingPlanDto, cancellationToken);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _trainingPlanService.DeleteTrainingPlanAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    [HttpPost("{trainingPlanId}/planned-activities")]
    public async Task<ActionResult<PlannedActivityDto>> AddPlannedActivity(int trainingPlanId,
        CreatePlannedActivityDto createPlannedActivityDto, CancellationToken cancellationToken)
    {
        if (trainingPlanId != createPlannedActivityDto.TrainingPlanId)
            return BadRequest("TrainingPlanID in der URL stimmt nicht mit der ID im Körper überein.");

        try
        {
            var plannedActivity = await _trainingPlanService.AddPlannedActivityAsync(trainingPlanId, createPlannedActivityDto,
                cancellationToken);
            return Ok(plannedActivity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("planned-activities/{plannedActivityId}")]
    public async Task<IActionResult> UpdatePlannedActivity(int plannedActivityId, UpdatePlannedActivityDto updatePlannedActivityDto, CancellationToken cancellationToken)
    {
        if (plannedActivityId != updatePlannedActivityDto.Id)
            return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

        try
        {
            await _trainingPlanService.UpdatePlannedActivityAsync(updatePlannedActivityDto, cancellationToken);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    [HttpDelete("planned-activities/{plannedActivityId}")]
    public async Task<IActionResult> DeletePlannedActivity(int plannedActivityId, CancellationToken cancellationToken)
    {
        try
        {
            await _trainingPlanService.DeletePlannedActivityAsync(plannedActivityId, cancellationToken);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    [HttpPost("planned-activities/{plannedActivityId}/complete")]
    public async Task<ActionResult<PlannedActivityDto>> MarkPlannedActivityAsCompleted(int plannedActivityId, int activityId, CancellationToken cancellationToken)
    {
        try
        {
            var plannedActivity = await _trainingPlanService.MarkPlannedActivityAsCompletedAsync(plannedActivityId, activityId, cancellationToken);
            return Ok(plannedActivity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
