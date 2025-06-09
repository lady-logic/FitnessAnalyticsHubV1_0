using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Application.Services;

public class AthleteService : IAthleteService
{
    private readonly IApplicationDbContext _context;
    private readonly IStravaService _stravaService;
    private readonly IMapper _mapper;

    public AthleteService(
        IApplicationDbContext context,
        IStravaService stravaService,
        IMapper mapper)
    {
        _context = context;
        _stravaService = stravaService;
        _mapper = mapper;
    }

    public async Task<AthleteDto> GetAthleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (athlete == null)
            throw new AthleteNotFoundException(id);

        return _mapper.Map<AthleteDto>(athlete);
    }

    public async Task<IEnumerable<AthleteDto>> GetAllAthletesAsync(CancellationToken cancellationToken)
    {
        var athletes = await _context.Athletes.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<AthleteDto>>(athletes);
    }

    public async Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = _mapper.Map<Athlete>(athleteDto);
        await _context.Athletes.AddAsync(athlete, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AthleteDto>(athlete);
    }

    public async Task UpdateAthleteAsync(UpdateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes.FirstOrDefaultAsync(a => a.Id == athleteDto.Id, cancellationToken);

        if (athlete == null)
            throw new AthleteNotFoundException(athleteDto.Id);

        _mapper.Map(athleteDto, athlete);
        athlete.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAthleteAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (athlete == null)
            throw new AthleteNotFoundException(id);

        _context.Athletes.Remove(athlete);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken, CancellationToken cancellationToken)
    {
        var stravaAthlete = await _stravaService.GetAthleteProfileAsync(accessToken);

        // Check if athlete already exists
        var existingAthlete = await _context.Athletes.FirstOrDefaultAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);

        if (existingAthlete != null)
        {
            // Update existing athlete
            _mapper.Map(stravaAthlete, existingAthlete);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<AthleteDto>(existingAthlete);
        }
        else
        {
            // Create new athlete
            var newAthlete = _mapper.Map<Athlete>(stravaAthlete);
            newAthlete.CreatedAt = DateTime.Now;
            newAthlete.UpdatedAt = DateTime.Now;

            await _context.Athletes.AddAsync(newAthlete, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<AthleteDto>(newAthlete);
        }
    }
}
