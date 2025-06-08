using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces;

public interface IAthleteService
{
    Task<AthleteDto?> GetAthleteByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<AthleteDto>> GetAllAthletesAsync(CancellationToken cancellationToken);
    Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto, CancellationToken cancellationToken);
    Task UpdateAthleteAsync(UpdateAthleteDto athleteDto, CancellationToken cancellationToken);
    Task DeleteAthleteAsync(int id, CancellationToken cancellationToken);
    Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken, CancellationToken cancellationToken);
}
