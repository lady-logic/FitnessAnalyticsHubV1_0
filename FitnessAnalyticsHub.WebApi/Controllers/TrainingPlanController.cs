using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers
{
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
        public async Task<ActionResult<TrainingPlanDto>> GetById(int id)
        {
            var trainingPlan = await _trainingPlanService.GetTrainingPlanByIdAsync(id);
            if (trainingPlan == null)
                return NotFound($"Trainingsplan mit ID {id} wurde nicht gefunden.");
            return Ok(trainingPlan);
        }

        [HttpGet("athlete/{athleteId}")]
        public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetByAthleteId(int athleteId)
        {
            var trainingPlans = await _trainingPlanService.GetTrainingPlansByAthleteIdAsync(athleteId);
            return Ok(trainingPlans);
        }

        [HttpPost]
        public async Task<ActionResult<TrainingPlanDto>> Create(CreateTrainingPlanDto createTrainingPlanDto)
        {
            var trainingPlan = await _trainingPlanService.CreateTrainingPlanAsync(createTrainingPlanDto);
            return CreatedAtAction(nameof(GetById), new { id = trainingPlan.Id }, trainingPlan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTrainingPlanDto updateTrainingPlanDto)
        {
            if (id != updateTrainingPlanDto.Id)
                return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

            try
            {
                await _trainingPlanService.UpdateTrainingPlanAsync(updateTrainingPlanDto);
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
                await _trainingPlanService.DeleteTrainingPlanAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpPost("{trainingPlanId}/planned-activities")]
        public async Task<ActionResult<PlannedActivityDto>> AddPlannedActivity(int trainingPlanId, CreatePlannedActivityDto createPlannedActivityDto)
        {
            if (trainingPlanId != createPlannedActivityDto.TrainingPlanId)
                return BadRequest("TrainingPlanID in der URL stimmt nicht mit der ID im Körper überein.");

            try
            {
                var plannedActivity = await _trainingPlanService.AddPlannedActivityAsync(trainingPlanId, createPlannedActivityDto);
                return Ok(plannedActivity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("planned-activities/{plannedActivityId}")]
        public async Task<IActionResult> UpdatePlannedActivity(int plannedActivityId, UpdatePlannedActivityDto updatePlannedActivityDto)
        {
            if (plannedActivityId != updatePlannedActivityDto.Id)
                return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

            try
            {
                await _trainingPlanService.UpdatePlannedActivityAsync(updatePlannedActivityDto);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("planned-activities/{plannedActivityId}")]
        public async Task<IActionResult> DeletePlannedActivity(int plannedActivityId)
        {
            try
            {
                await _trainingPlanService.DeletePlannedActivityAsync(plannedActivityId);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpPost("planned-activities/{plannedActivityId}/complete")]
        public async Task<ActionResult<PlannedActivityDto>> MarkPlannedActivityAsCompleted(int plannedActivityId, int activityId)
        {
            try
            {
                var plannedActivity = await _trainingPlanService.MarkPlannedActivityAsCompletedAsync(plannedActivityId, activityId);
                return Ok(plannedActivity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
