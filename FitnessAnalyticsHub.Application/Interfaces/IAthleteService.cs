using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces
{
    public interface IAthleteService
    {
        Task<AthleteDto?> GetAthleteByIdAsync(int id);
        Task<IEnumerable<AthleteDto>> GetAllAthletesAsync();
        Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto);
        Task UpdateAthleteAsync(UpdateAthleteDto athleteDto);
        Task DeleteAthleteAsync(int id);
        Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken);
    }
}
