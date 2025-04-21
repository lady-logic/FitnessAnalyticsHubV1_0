using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IRepository<Activity> _activityRepository;
        private readonly IRepository<Athlete> _athleteRepository;
        private readonly IStravaService _stravaService;
        private readonly IMapper _mapper;

        public ActivityService(
        IRepository<Activity> activityRepository,
        IRepository<Athlete> athleteRepository,
        IStravaService stravaService,
        IMapper mapper)
        {
            _activityRepository = activityRepository;
            _athleteRepository = athleteRepository;
            _stravaService = stravaService;
            _mapper = mapper;
        }

        public async Task<ActivityDto?> GetActivityByIdAsync(int id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null)
                return null;

            var activityDto = _mapper.Map<ActivityDto>(activity);

            if (activity.Athlete != null)
            {
                activityDto.AthleteFullName = $"{activity.Athlete.FirstName} {activity.Athlete.LastName}";
            }

            return activityDto;
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId)
        {
            var activities = await _activityRepository.FindAsync(a => a.AthleteId == athleteId);

            var athlete = await _athleteRepository.GetByIdAsync(athleteId);
            var athleteFullName = athlete != null ? $"{athlete.FirstName} {athlete.LastName}" : string.Empty;

            var activityDtos = _mapper.Map<IEnumerable<ActivityDto>>(activities).ToList();

            foreach (var activityDto in activityDtos)
            {
                activityDto.AthleteFullName = athleteFullName;
            }

            return activityDtos;
        }

        public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto)
        {
            var activity = _mapper.Map<Activity>(activityDto);

            await _activityRepository.AddAsync(activity);
            await _activityRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<ActivityDto>(activity);

            var athlete = await _athleteRepository.GetByIdAsync(activity.AthleteId);
            if (athlete != null)
            {
                resultDto.AthleteFullName = $"{athlete.FirstName} {athlete.LastName}";
            }

            return resultDto;
        }

        public async Task UpdateActivityAsync(UpdateActivityDto activityDto)
        {
            var activity = await _activityRepository.GetByIdAsync(activityDto.Id);
            if (activity == null)
                throw new Exception($"Activity with ID {activityDto.Id} not found");

            _mapper.Map(activityDto, activity);
            activity.UpdatedAt = DateTime.Now;

            await _activityRepository.UpdateAsync(activity);
            await _activityRepository.SaveChangesAsync();
        }

        public async Task DeleteActivityAsync(int id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null)
                throw new Exception($"Activity with ID {id} not found");

            await _activityRepository.DeleteAsync(activity);
            await _activityRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(int athleteId, string accessToken)
        {
            var athlete = await _athleteRepository.GetByIdAsync(athleteId);
            if (athlete == null)
                throw new Exception($"Athlete with ID {athleteId} not found");

            var stravaActivities = await _stravaService.GetActivitiesAsync(accessToken);
            var importedActivities = new List<ActivityDto>();

            foreach (var stravaActivity in stravaActivities)
            {
                // Check if activity already exists
                var existingActivities = await _activityRepository.FindAsync(a =>
                    a.AthleteId == athleteId && a.StravaId == stravaActivity.StravaId);

                var existingActivity = existingActivities.FirstOrDefault();

                if (existingActivity == null)
                {
                    // Create new activity
                    var newActivity = new Activity
                    {
                        AthleteId = athleteId,
                        StravaId = stravaActivity.StravaId,
                        Name = stravaActivity.Name,
                        Description = null, // Strava API might not provide this directly
                        Distance = stravaActivity.Distance,
                        MovingTime = stravaActivity.MovingTime,
                        ElapsedTime = stravaActivity.ElapsedTime,
                        TotalElevationGain = stravaActivity.TotalElevationGain,
                        SportType = stravaActivity.SportType,
                        StartDate = stravaActivity.StartDate,
                        StartDateLocal = stravaActivity.StartDateLocal,
                        Timezone = stravaActivity.Timezone,
                        AverageSpeed = stravaActivity.AverageSpeed,
                        MaxSpeed = stravaActivity.MaxSpeed,
                        AverageHeartRate = stravaActivity.AverageHeartRate,
                        MaxHeartRate = stravaActivity.MaxHeartRate,
                        AveragePower = stravaActivity.AveragePower,
                        MaxPower = stravaActivity.MaxPower,
                        AverageCadence = stravaActivity.AverageCadence,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _activityRepository.AddAsync(newActivity);

                    var activityDto = _mapper.Map<ActivityDto>(newActivity);
                    activityDto.AthleteFullName = $"{athlete.FirstName} {athlete.LastName}";
                    importedActivities.Add(activityDto);
                }
            }

            await _activityRepository.SaveChangesAsync();
            return importedActivities;
        }

        public async Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId)
        {
            var activities = await _activityRepository.FindAsync(a => a.AthleteId == athleteId);
            var activitiesList = activities.ToList();

            if (!activitiesList.Any())
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

            var stats = new ActivityStatisticsDto
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

            return stats;
        }

    }
}
