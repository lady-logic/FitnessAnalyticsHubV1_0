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
    private readonly IApplicationDbContext context;
    private readonly IStravaService stravaService;
    private readonly IMapper mapper;

    public AthleteService(
        IApplicationDbContext context,
        IStravaService stravaService,
        IMapper mapper)
    {
        this.context = context;
        this.stravaService = stravaService;
        this.mapper = mapper;
    }

    public async Task<AthleteDto> GetAthleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await this.context.Athletes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (athlete == null)
        {
            throw new AthleteNotFoundException(id);
        }

        return this.mapper.Map<AthleteDto>(athlete);
    }

    public async Task<IEnumerable<AthleteDto>> GetAllAthletesAsync(CancellationToken cancellationToken)
    {
        var athletes = await this.context.Athletes.ToListAsync(cancellationToken);
        return this.mapper.Map<IEnumerable<AthleteDto>>(athletes);
    }

    public async Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = this.mapper.Map<Athlete>(athleteDto);
        await this.context.Athletes.AddAsync(athlete, cancellationToken);
        await this.context.SaveChangesAsync(cancellationToken);
        return this.mapper.Map<AthleteDto>(athlete);
    }

    public async Task UpdateAthleteAsync(UpdateAthleteDto athleteDto, CancellationToken cancellationToken)
    {
        var athlete = await this.context.Athletes.FirstOrDefaultAsync(a => a.Id == athleteDto.Id, cancellationToken);

        if (athlete == null)
        {
            throw new AthleteNotFoundException(athleteDto.Id);
        }

        this.mapper.Map(athleteDto, athlete);
        athlete.UpdatedAt = DateTime.Now;

        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAthleteAsync(int id, CancellationToken cancellationToken)
    {
        var athlete = await this.context.Athletes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (athlete == null)
        {
            throw new AthleteNotFoundException(id);
        }

        this.context.Athletes.Remove(athlete);
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken, CancellationToken cancellationToken)
    {
        var stravaAthlete = await this.stravaService.GetAthleteProfileAsync(accessToken);

        // Check if athlete already exists
        var existingAthlete = await this.context.Athletes.FirstOrDefaultAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);

        if (existingAthlete != null)
        {
            // Update existing athlete
            this.mapper.Map(stravaAthlete, existingAthlete);
            await this.context.SaveChangesAsync(cancellationToken);
            return this.mapper.Map<AthleteDto>(existingAthlete);
        }
        else
        {
            // Create new athlete
            var newAthlete = this.mapper.Map<Athlete>(stravaAthlete);
            newAthlete.CreatedAt = DateTime.Now;
            newAthlete.UpdatedAt = DateTime.Now;

            await this.context.Athletes.AddAsync(newAthlete, cancellationToken);
            await this.context.SaveChangesAsync(cancellationToken);

            return this.mapper.Map<AthleteDto>(newAthlete);
        }
    }
}
