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
    private readonly IApplicationDbContext _context;
    private readonly IStravaService _stravaService;
    private readonly IAIAssistantClientService _aiClient;
    private readonly IMapper _mapper;

    public ActivityService(
    IApplicationDbContext context,
    IStravaService stravaService,
    IAIAssistantClientService aiClient,
    IMapper mapper)
    {
        _context = context;
        _stravaService = stravaService;
        _aiClient = aiClient;
        _mapper = mapper;
    }

    public async Task<ActivityDto> GetActivityByIdAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await _context.Activities
            .Include(a => a.Athlete)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity == null)
            throw new ActivityNotFoundException(id);

        var activityDto = _mapper.Map<ActivityDto>(activity);

        return activityDto;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        var activities = await _context.Activities
            .Include(a => a.Athlete)
            .Where(a => a.AthleteId == athleteId)
            .ToListAsync(cancellationToken);

        var activityDtos = _mapper.Map<IEnumerable<ActivityDto>>(activities);
        return activityDtos;
    }

    public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = _mapper.Map<Activity>(activityDto);

        await _context.Activities.AddAsync(activity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Activity mit Athlete laden für das Mapping
        var activityWithAthlete = await _context.Activities
            .Include(a => a.Athlete)
            .FirstAsync(a => a.Id == activity.Id, cancellationToken);

        var resultDto = _mapper.Map<ActivityDto>(activityWithAthlete);
        return resultDto;
    }

    public async Task UpdateActivityAsync(UpdateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == activityDto.Id, cancellationToken);

        if (activity == null)
            throw new ActivityNotFoundException(activityDto.Id);

        _mapper.Map(activityDto, activity);
        activity.UpdatedAt = DateTime.Now;

        // _context.Activities.Update(activity); // Nicht nötig - EF Core tracked automatisch!
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteActivityAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity == null)
            throw new ActivityNotFoundException(id);

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== IMPORTING YOUR STRAVA ACTIVITIES ===");

        // StravaService aufrufen
        var (stravaAthlete, stravaActivities) = await _stravaService.ImportMyActivitiesAsync();

        // Athlet in DB finden oder erstellen
        var athlete = await _context.Athletes.FirstOrDefaultAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);

        if (athlete == null)
        {
            // Neuen Athleten erstellen
            athlete = _mapper.Map<Athlete>(stravaAthlete);
            athlete.CreatedAt = DateTime.Now;
            athlete.UpdatedAt = DateTime.Now;

            await _context.Athletes.AddAsync(athlete, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"✅ Created new athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }
        else
        {
            Console.WriteLine($"✅ Found existing athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }

        // Aktivitäten verarbeiten und speichern
        // BULK CHECK: Alle existierenden StravaIds auf einmal abfragen
        var stravaIds = stravaActivities.Select(sa => sa.StravaId).ToList();
        var existingStravaIds = await _context.Activities
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
            var newActivity = _mapper.Map<Activity>(stravaActivity);
            newActivity.AthleteId = athlete.Id;
            newActivity.Athlete = athlete; // Für das AthleteFullName Mapping

            await _context.Activities.AddAsync(newActivity, cancellationToken);

            var activityDto = _mapper.Map<ActivityDto>(newActivity);
            importedActivities.Add(activityDto);

            Console.WriteLine($"✅ Imported: {newActivity.Name} ({newActivity.SportType}, {newActivity.Distance / 1000:F1}km)");
        }

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"=== IMPORT COMPLETE ===");
        Console.WriteLine($"Total activities from Strava: {stravaActivities.Count()}");
        Console.WriteLine($"New activities imported: {importedActivities.Count}");

        return importedActivities;
    }

    public async Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId, CancellationToken cancellationToken)
    {
        // Prüfen ob Athlet existiert
        var athleteExists = await _context.Athletes.AnyAsync(a => a.Id == athleteId, cancellationToken);

        if (!athleteExists)
            throw new AthleteNotFoundException(athleteId);

        var activities = await _context.Activities
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
            .ToDictionary(g => g.Key, g => g.Count())
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
            ActivitiesByMonth = new Dictionary<int, int>()
        };
    }
}
