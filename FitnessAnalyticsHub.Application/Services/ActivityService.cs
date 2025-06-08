using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;

namespace FitnessAnalyticsHub.Application.Services;

public class ActivityService : IActivityService
{
    private readonly IRepository<Activity> _activityRepository;
    private readonly IRepository<Athlete> _athleteRepository;
    private readonly IStravaService _stravaService;
    //private readonly IAIAssistantClient _aiClient;
    private readonly IMapper _mapper;

    public ActivityService(
    IRepository<Activity> activityRepository,
    IRepository<Athlete> athleteRepository,
    IStravaService stravaService,
    IAIAssistantClient aiClient,
    IMapper mapper)
    {
        _activityRepository = activityRepository;
        _athleteRepository = athleteRepository;
        _stravaService = stravaService;
        //_aiClient = aiClient;
        _mapper = mapper;
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return null;

        var activityDto = _mapper.Map<ActivityDto>(activity);

        return activityDto;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.FindAsync(a => a.AthleteId == athleteId, cancellationToken);

        var athlete = await _athleteRepository.GetByIdAsync(athleteId, cancellationToken);
        var athleteFullName = athlete != null ? $"{athlete.FirstName} {athlete.LastName}" : string.Empty;

        var activityDtos = _mapper.Map<IEnumerable<ActivityDto>>(activities).ToList();

        foreach (var activityDto in activityDtos)
        {
            activityDto.AthleteFullName = athleteFullName;
        }

        return activityDtos;
    }

    public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = _mapper.Map<Activity>(activityDto);

        await _activityRepository.AddAsync(activity, cancellationToken);
        await _activityRepository.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<ActivityDto>(activity);

        var athlete = await _athleteRepository.GetByIdAsync(activity.AthleteId, cancellationToken);
        if (athlete != null)
        {
            resultDto.AthleteFullName = $"{athlete.FirstName} {athlete.LastName}";
        }

        return resultDto;
    }

    public async Task UpdateActivityAsync(UpdateActivityDto activityDto, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(activityDto.Id, cancellationToken);
        if (activity == null)
            throw new Exception($"Activity with ID {activityDto.Id} not found");

        _mapper.Map(activityDto, activity);
        activity.UpdatedAt = DateTime.Now;

        await _activityRepository.UpdateAsync(activity);
        await _activityRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteActivityAsync(int id, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            throw new Exception($"Activity with ID {id} not found");

        await _activityRepository.DeleteAsync(activity);
        await _activityRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== IMPORTING YOUR STRAVA ACTIVITIES ===");

        // StravaService aufrufen
        var (stravaAthlete, stravaActivities) = await _stravaService.ImportMyActivitiesAsync();

        // Athlet in DB finden oder erstellen
        var existingAthletes = await _athleteRepository.FindAsync(a => a.StravaId == stravaAthlete.StravaId, cancellationToken);
        var athlete = existingAthletes.FirstOrDefault();

        if (athlete == null)
        {
            // Neuen Athleten erstellen
            athlete = _mapper.Map<Athlete>(stravaAthlete);
            athlete.CreatedAt = DateTime.Now;
            athlete.UpdatedAt = DateTime.Now;

            await _athleteRepository.AddAsync(athlete, cancellationToken);
            await _athleteRepository.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"✅ Created new athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }
        else
        {
            Console.WriteLine($"✅ Found existing athlete: {athlete.FirstName} {athlete.LastName} (ID: {athlete.Id})");
        }

        // Aktivitäten verarbeiten und speichern
        var importedActivities = new List<ActivityDto>();

        foreach (var stravaActivity in stravaActivities)
        {
            // Prüfen ob Aktivität bereits existiert
            var existingActivities = await _activityRepository.FindAsync(a =>
                a.AthleteId == athlete.Id && a.StravaId == stravaActivity.StravaId, cancellationToken);

            if (!existingActivities.Any())
            {
                // Neue Aktivität erstellen
                var newActivity = _mapper.Map<Activity>(stravaActivity);
                newActivity.AthleteId = athlete.Id;
                newActivity.Athlete = athlete; // Für das AthleteFullName Mapping

                await _activityRepository.AddAsync(newActivity, cancellationToken);

                var activityDto = _mapper.Map<ActivityDto>(newActivity);
                importedActivities.Add(activityDto);

                Console.WriteLine($"✅ Imported: {newActivity.Name} ({newActivity.SportType}, {newActivity.Distance / 1000:F1}km)");
            }
        }

        await _activityRepository.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"=== IMPORT COMPLETE ===");
        Console.WriteLine($"Total activities from Strava: {stravaActivities.Count()}");
        Console.WriteLine($"New activities imported: {importedActivities.Count}");

        return importedActivities;
    }

    public async Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.FindAsync(a => a.AthleteId == athleteId, cancellationToken);
        var activitiesList = activities.ToList();

        if (!activitiesList.Any())
        {
            return CreateEmptyStatistics();
        }

        return new ActivityStatisticsDto
        {
            TotalActivities = activitiesList.Count,
            TotalDistance = activitiesList.Sum(a => a.Distance),
            TotalDuration = TimeSpan.FromSeconds(activitiesList.Sum(a => a.MovingTime)),
            TotalElevationGain = activitiesList.Sum(a => a.TotalElevationGain),
            ActivitiesByType = activitiesList
                .GroupBy(a => a.SportType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ActivitiesByMonth = activitiesList
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
