using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Application.Services;

public class ActivityService : IActivityService
{
    private readonly IApplicationDbContext context;
    private readonly IStravaService stravaService;
    private readonly IMapper mapper;

    public ActivityService(
    IApplicationDbContext context,
    IStravaService stravaService,
    IMapper mapper)
    {
        this.context = context;
        this.stravaService = stravaService;
        this.mapper = mapper;
    }

    public async Task<ActivityDto> GetActivityByIdAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await this.context.Activities
            .Include(a => a.Athlete)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity == null)
        {
            throw new ActivityNotFoundException(id);
        }

        var activityDto = this.mapper.Map<ActivityDto>(activity);

        return activityDto;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        var activities = await this.context.Activities
            .Include(a => a.Athlete)
            .Where(a => a.AthleteId == athleteId)
            .ToListAsync(cancellationToken);

        var activityDtos = this.mapper.Map<IEnumerable<ActivityDto>>(activities);
        return activityDtos;
    }

    public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = this.mapper.Map<Activity>(activityDto);

        await this.context.Activities.AddAsync(activity, cancellationToken);
        await this.context.SaveChangesAsync(cancellationToken);

        // Activity mit Athlete laden für das Mapping
        var activityWithAthlete = await this.context.Activities
            .Include(a => a.Athlete)
            .FirstAsync(a => a.Id == activity.Id, cancellationToken);

        var resultDto = this.mapper.Map<ActivityDto>(activityWithAthlete);
        return resultDto;
    }

    public async Task UpdateActivityAsync(UpdateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = await this.context.Activities.FirstOrDefaultAsync(a => a.Id == activityDto.Id, cancellationToken);

        if (activity == null)
        {
            throw new ActivityNotFoundException(activityDto.Id);
        }

        this.mapper.Map(activityDto, activity);
        activity.UpdatedAt = DateTime.Now;

        // _context.Activities.Update(activity); // Nicht nötig - EF Core tracked automatisch!
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteActivityAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await this.context.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity == null)
        {
            throw new ActivityNotFoundException(id);
        }

        this.context.Activities.Remove(activity);
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== IMPORTING YOUR STRAVA ACTIVITIES ===");

        // StravaService aufrufen
        var (stravaAthlete, stravaActivities) = await this.stravaService.ImportMyActivitiesAsync();

        // Athlet in DB finden oder erstellen
        var athlete = await this.context.Athletes.FirstOrDefaultAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);

        if (athlete == null)
        {
            // Neuen Athleten erstellen
            athlete = this.mapper.Map<Athlete>(stravaAthlete);
            athlete.CreatedAt = DateTime.Now;
            athlete.UpdatedAt = DateTime.Now;

            await this.context.Athletes.AddAsync(athlete, cancellationToken);
            await this.context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"✅ Created new athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }
        else
        {
            Console.WriteLine($"✅ Found existing athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }

        // Aktivitäten verarbeiten und speichern
        // BULK CHECK: Alle existierenden StravaIds auf einmal abfragen
        var stravaIds = stravaActivities.Select(sa => sa.StravaId).ToList();
        var existingStravaIds = await this.context.Activities
            .Where(a => a.AthleteId == athlete.Id && stravaIds.Contains(a.StravaId))
            .Select(a => a.StravaId)
            .ToListAsync(cancellationToken);

        // Nur neue Aktivitäten verarbeiten
        var newStravaActivities = stravaActivities
            .Where(sa => !existingStravaIds.Contains(sa.StravaId))
            .ToList();

        var importedActivities = new List<ActivityDto>();

        foreach (var stravaActivity in newStravaActivities)
        {
            var newActivity = this.mapper.Map<Activity>(stravaActivity);
            newActivity.AthleteId = athlete.Id;
            newActivity.Athlete = athlete; // Für das AthleteFullName Mapping

            await this.context.Activities.AddAsync(newActivity, cancellationToken);

            var activityDto = this.mapper.Map<ActivityDto>(newActivity);
            importedActivities.Add(activityDto);

            Console.WriteLine($"✅ Imported: {newActivity.Name} ({newActivity.SportType}, {newActivity.Distance / 1000:F1}km)");
        }

        await this.context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"=== IMPORT COMPLETE ===");
        Console.WriteLine($"Total activities from Strava: {stravaActivities.Count()}");
        Console.WriteLine($"New activities imported: {importedActivities.Count}");

        return importedActivities;
    }

    public async Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId, CancellationToken cancellationToken)
    {
        // Prüfen ob Athlet existiert
        var athleteExists = await this.context.Athletes.AnyAsync(a => a.Id == athleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new AthleteNotFoundException(athleteId);
        }

        var activities = await this.context.Activities
            .Where(a => a.AthleteId == athleteId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
        {
            return CreateEmptyStatistics();
        }

        return new ActivityStatisticsDto
        {
            TotalActivities = activities.Count,
            TotalDistance = activities.Sum(a => a.Distance),
            TotalDuration = TimeSpan.FromSeconds(activities.Sum(a => a.MovingTime)),
            TotalElevationGain = activities.Sum(a => a.TotalElevationGain),
            ActivitiesByType = activities
            .GroupBy(a => a.SportType)
            .ToDictionary(g => g.Key, g => g.Count()),
            ActivitiesByMonth = activities
            .GroupBy(a => a.StartDateLocal.Month)
            .ToDictionary(g => g.Key, g => g.Count()),
        };
    }

    private static ActivityStatisticsDto CreateEmptyStatistics()
    {
        return new ActivityStatisticsDto
        {
            TotalActivities = 0,
            TotalDistance = 0,
            TotalDuration = TimeSpan.Zero,
            TotalElevationGain = 0,
            ActivitiesByType = new Dictionary<string, int>(),
            ActivitiesByMonth = new Dictionary<int, int>(),
        };
    }
}
