namespace FitnessAnalyticsHub.WebApi.Controllers;

using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService activityService;

    public ActivityController(IActivityService activityService)
    {
        this.activityService = activityService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        ActivityDto? activity = await this.activityService.GetActivityByIdAsync(id, cancellationToken);
        return this.Ok(activity);
    }

    [HttpGet("athlete/{athleteId}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByAthleteId(int athleteId, CancellationToken cancellationToken)
    {
        IEnumerable<ActivityDto> activities = await this.activityService.GetActivitiesByAthleteIdAsync(athleteId, cancellationToken);
        return this.Ok(activities);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(CreateActivityDto createActivityDto, CancellationToken cancellationToken)
    {
        ActivityDto activity = await this.activityService.CreateActivityAsync(createActivityDto, cancellationToken);
        return this.CreatedAtAction(nameof(this.GetById), new { id = activity.Id }, activity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateActivityDto updateActivityDto, CancellationToken cancellationToken)
    {
        if (id != updateActivityDto.Id)
        {
            return this.BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");
        }

        await this.activityService.UpdateActivityAsync(updateActivityDto, cancellationToken);
        return this.NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await this.activityService.DeleteActivityAsync(id, cancellationToken);
        return this.NoContent();
    }

    [HttpPost("import-from-strava")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> ImportFromStrava(CancellationToken cancellationToken)
    {
        IEnumerable<ActivityDto> activities = await this.activityService.ImportActivitiesFromStravaAsync(cancellationToken);
        return this.Ok(activities);
    }

    [HttpGet("statistics/{athleteId}")]
    public async Task<ActionResult<ActivityStatisticsDto>> GetStatistics(int athleteId, CancellationToken cancellationToken)
    {
        ActivityStatisticsDto statistics = await this.activityService.GetAthleteActivityStatisticsAsync(athleteId, cancellationToken);
        return this.Ok(statistics);
    }
}
