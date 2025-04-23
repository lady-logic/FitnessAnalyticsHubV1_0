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
            if (activity == null)
                return NotFound($"Aktivität mit ID {id} wurde nicht gefunden.");
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

            try
            {
                await _activityService.UpdateActivityAsync(updateActivityDto);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _activityService.DeleteActivityAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpPost("import-from-strava")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> ImportFromStrava(int athleteId, string accessToken)
        {
            try
            {
                var activities = await _activityService.ImportActivitiesFromStravaAsync(athleteId, accessToken);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("statistics/{athleteId}")]
        public async Task<ActionResult<ActivityStatisticsDto>> GetStatistics(int athleteId)
        {
            try
            {
                var statistics = await _activityService.GetAthleteActivityStatisticsAsync(athleteId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
