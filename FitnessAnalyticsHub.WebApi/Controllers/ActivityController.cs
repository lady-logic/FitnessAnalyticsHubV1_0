using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers
{
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
        public async Task<ActionResult<ActivityDto>> GetById(int id)
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            return Ok(activity);
        }

        [HttpGet("athlete/{athleteId}")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByAthleteId(int athleteId)
        {
            var activities = await _activityService.GetActivitiesByAthleteIdAsync(athleteId);
            return Ok(activities);
        }

        [HttpPost]
        public async Task<ActionResult<ActivityDto>> Create(CreateActivityDto createActivityDto)
        {
            var activity = await _activityService.CreateActivityAsync(createActivityDto);
            return CreatedAtAction(nameof(GetById), new { id = activity.Id }, activity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateActivityDto updateActivityDto)
        {
            if (id != updateActivityDto.Id)
                return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");


            await _activityService.UpdateActivityAsync(updateActivityDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _activityService.DeleteActivityAsync(id);
            return NoContent();
        }

        [HttpPost("import-from-strava")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> ImportFromStrava()
        {
            var activities = await _activityService.ImportActivitiesFromStravaAsync();
            return Ok(activities);
        }

        [HttpGet("statistics/{athleteId}")]
        public async Task<ActionResult<ActivityStatisticsDto>> GetStatistics(int athleteId)
        {
            var statistics = await _activityService.GetAthleteActivityStatisticsAsync(athleteId);
            return Ok(statistics);
        }
    }
}
