namespace FitnessAnalyticsHub.WebApi.Controllers;

using FitnessAnalyticsHub.Application;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TrainingPlanController : ControllerBase
{
    private readonly ITrainingPlanService trainingPlanService;

    public TrainingPlanController(ITrainingPlanService trainingPlanService)
    {
        this.trainingPlanService = trainingPlanService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingPlanDto>> GetById(int id, CancellationToken cancellationToken)
    {
        TrainingPlanDto? trainingPlan = await this.trainingPlanService.GetTrainingPlanByIdAsync(id, cancellationToken);
        if (trainingPlan == null)
        {
            return this.NotFound($"Trainingsplan mit ID {id} wurde nicht gefunden.");
        }

        return this.Ok(trainingPlan);
    }

    [HttpGet("athlete/{athleteId}")]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetByAthleteId(int athleteId, CancellationToken cancellationToken)
    {
        IEnumerable<TrainingPlanDto> trainingPlans = await this.trainingPlanService.GetTrainingPlansByAthleteIdAsync(athleteId, cancellationToken);
        return this.Ok(trainingPlans);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingPlanDto>> Create(CreateTrainingPlanDto createTrainingPlanDto, CancellationToken cancellationToken)
    {
        TrainingPlanDto trainingPlan = await this.trainingPlanService.CreateTrainingPlanAsync(createTrainingPlanDto, cancellationToken);
        return this.CreatedAtAction(nameof(this.GetById), new { id = trainingPlan.Id }, trainingPlan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTrainingPlanDto updateTrainingPlanDto, CancellationToken cancellationToken)
    {
        if (id != updateTrainingPlanDto.Id)
        {
            return this.BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");
        }

        try
        {
            await this.trainingPlanService.UpdateTrainingPlanAsync(updateTrainingPlanDto, cancellationToken);
        }
        catch (Exception ex)
        {
            return this.NotFound(ex.Message);
        }

        return this.NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await this.trainingPlanService.DeleteTrainingPlanAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            return this.NotFound(ex.Message);
        }

        return this.NoContent();
    }

    [HttpPost("{trainingPlanId}/planned-activities")]
    public async Task<ActionResult<PlannedActivityDto>> AddPlannedActivity(
        int trainingPlanId,
        CreatePlannedActivityDto createPlannedActivityDto, CancellationToken cancellationToken)
    {
        if (trainingPlanId != createPlannedActivityDto.TrainingPlanId)
        {
            return this.BadRequest("TrainingPlanID in der URL stimmt nicht mit der ID im Körper überein.");
        }

        try
        {
            PlannedActivityDto plannedActivity = await this.trainingPlanService.AddPlannedActivityAsync(trainingPlanId, createPlannedActivityDto,
                cancellationToken);
            return this.Ok(plannedActivity);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }
    }

    [HttpPut("planned-activities/{plannedActivityId}")]
    public async Task<IActionResult> UpdatePlannedActivity(int plannedActivityId, UpdatePlannedActivityDto updatePlannedActivityDto, CancellationToken cancellationToken)
    {
        if (plannedActivityId != updatePlannedActivityDto.Id)
        {
            return this.BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");
        }

        try
        {
            await this.trainingPlanService.UpdatePlannedActivityAsync(updatePlannedActivityDto, cancellationToken);
        }
        catch (Exception ex)
        {
            return this.NotFound(ex.Message);
        }

        return this.NoContent();
    }

    [HttpDelete("planned-activities/{plannedActivityId}")]
    public async Task<IActionResult> DeletePlannedActivity(int plannedActivityId, CancellationToken cancellationToken)
    {
        try
        {
            await this.trainingPlanService.DeletePlannedActivityAsync(plannedActivityId, cancellationToken);
        }
        catch (Exception ex)
        {
            return this.NotFound(ex.Message);
        }

        return this.NoContent();
    }

    [HttpPost("planned-activities/{plannedActivityId}/complete")]
    public async Task<ActionResult<PlannedActivityDto>> MarkPlannedActivityAsCompleted(int plannedActivityId, int activityId, CancellationToken cancellationToken)
    {
        try
        {
            PlannedActivityDto plannedActivity = await this.trainingPlanService.MarkPlannedActivityAsCompletedAsync(plannedActivityId, activityId, cancellationToken);
            return this.Ok(plannedActivity);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }
    }
}
