using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var activity = await _activityService.GetActivityByIdAsync(id, cancellationToken);
        if (activity == null)
            return NotFound($"Aktivität mit ID {id} wurde nicht gefunden.");
        return Ok(activity);
    }

    [HttpGet("athlete/{athleteId}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByAthleteId(int athleteId, CancellationToken cancellationToken)
    {
        var activities = await _activityService.GetActivitiesByAthleteIdAsync(athleteId, cancellationToken);
        return Ok(activities);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(CreateActivityDto createActivityDto, CancellationToken cancellationToken)
    {
        var activity = await _activityService.CreateActivityAsync(createActivityDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, activity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateActivityDto updateActivityDto, CancellationToken cancellationToken)
    {
        if (id != updateActivityDto.Id)
            return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

        try
        {
            await _activityService.UpdateActivityAsync(updateActivityDto, cancellationToken);
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
            await _activityService.DeleteActivityAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    [HttpPost("import-from-strava")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> ImportFromStrava(CancellationToken cancellationToken)
    {
        try
        {
            var activities = await _activityService.ImportActivitiesFromStravaAsync(cancellationToken);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("statistics/{athleteId}")]
    public async Task<ActionResult<ActivityStatisticsDto>> GetStatistics(int athleteId, CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _activityService.GetAthleteActivityStatisticsAsync(athleteId, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
