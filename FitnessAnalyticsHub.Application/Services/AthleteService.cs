using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;

namespace FitnessAnalyticsHub.Application.Services;

public class AthleteService : IAthleteService
{
    private readonly IRepository<Athlete> _athleteRepository;
    private readonly IStravaService _stravaService;
    private readonly IMapper _mapper;

    public AthleteService(
        IRepository<Athlete> athleteRepository,
        IStravaService stravaService,
        IMapper mapper)
    {
        _athleteRepository = athleteRepository;
        _stravaService = stravaService;
        _mapper = mapper;
    }

    public async Task<AthleteDto?> GetAthleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByIdAsync(id, cancellationToken);
        return athlete != null ? _mapper.Map<AthleteDto>(athlete) : null;
    }

    public async Task<IEnumerable<AthleteDto>> GetAllAthletesAsync(CancellationToken cancellationToken)
    {
        var athletes = await _athleteRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<AthleteDto>>(athletes);
    }

    public async Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = _mapper.Map<Athlete>(athleteDto);
        await _athleteRepository.AddAsync(athlete, cancellationToken);
        await _athleteRepository.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AthleteDto>(athlete);
    }

    public async Task UpdateAthleteAsync(UpdateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByIdAsync(athleteDto.Id, cancellationToken);
        if (athlete == null)
            throw new Exception($"Athlete with ID {athleteDto.Id} not found");

        _mapper.Map(athleteDto, athlete);
        athlete.UpdatedAt = DateTime.Now;

        await _athleteRepository.UpdateAsync(athlete);
        await _athleteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAthleteAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByIdAsync(id, cancellationToken);
        if (athlete == null)
            throw new Exception($"Athlete with ID {id} not found");

        await _athleteRepository.DeleteAsync(athlete);
        await _athleteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken, CancellationToken cancellationToken)
    {
        var stravaAthlete = await _stravaService.GetAthleteProfileAsync(accessToken);

        // Check if athlete already exists
        var existingAthletes = await _athleteRepository.FindAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);
        var existingAthlete = existingAthletes.FirstOrDefault();

        if (existingAthlete != null)
        {
            // Update existing athlete
            _mapper.Map(stravaAthlete, existingAthlete);
            await _athleteRepository.UpdateAsync(existingAthlete);
            await _athleteRepository.SaveChangesAsync(cancellationToken);
            return _mapper.Map<AthleteDto>(existingAthlete);
        }
        else
        {
            // Create new athlete
            var newAthlete = _mapper.Map<Athlete>(stravaAthlete);
            newAthlete.CreatedAt = DateTime.Now;
            newAthlete.UpdatedAt = DateTime.Now;

            await _athleteRepository.AddAsync(newAthlete, cancellationToken);
            await _athleteRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<AthleteDto>(newAthlete);
        }
    }
}
