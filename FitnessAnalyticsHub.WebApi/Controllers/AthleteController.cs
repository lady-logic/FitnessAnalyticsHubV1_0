using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AthleteController : ControllerBase
    {
        private readonly IAthleteService _athleteService;

        public AthleteController(IAthleteService athleteService)
        {
            _athleteService = athleteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AthleteDto>>> GetAll()
        {
            var athletes = await _athleteService.GetAllAthletesAsync();
            return Ok(athletes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AthleteDto>> GetById(int id)
        {
            var athlete = await _athleteService.GetAthleteByIdAsync(id);
            if (athlete == null)
                return NotFound($"Athlet mit ID {id} wurde nicht gefunden.");
            return Ok(athlete);
        }

        [HttpPost]
        public async Task<ActionResult<AthleteDto>> Create(CreateAthleteDto createAthleteDto)
        {
            var athlete = await _athleteService.CreateAthleteAsync(createAthleteDto);
            return CreatedAtAction(nameof(GetById), new { id = athlete.Id }, athlete);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAthleteDto updateAthleteDto)
        {
            if (id != updateAthleteDto.Id)
                return BadRequest("ID in der URL stimmt nicht mit der ID im Körper überein.");

            try
            {
                await _athleteService.UpdateAthleteAsync(updateAthleteDto);
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
                await _athleteService.DeleteAthleteAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        [HttpPost("import-from-strava")]
        public async Task<ActionResult<AthleteDto>> ImportFromStrava(string accessToken)
        {
            try
            {
                var athlete = await _athleteService.ImportAthleteFromStravaAsync(accessToken);
                return Ok(athlete);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
