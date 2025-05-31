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


            await _athleteService.UpdateAthleteAsync(updateAthleteDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            await _athleteService.DeleteAthleteAsync(id);
            return NoContent();
        }

        [HttpPost("import-from-strava")]
        public async Task<ActionResult<AthleteDto>> ImportFromStrava(string accessToken)
        {
            var athlete = await _athleteService.ImportAthleteFromStravaAsync(accessToken);
            return Ok(athlete);
        }
    }
}
