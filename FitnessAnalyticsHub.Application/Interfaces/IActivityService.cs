namespace FitnessAnalyticsHub.Application.Interfaces;

using FitnessAnalyticsHub.Application.DTOs;

public interface IActivityService
{
    Task<ActivityDto?> GetActivityByIdAsync(int id, CancellationToken cancellationToken);

    Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId, CancellationToken cancellationToken);

    Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto, CancellationToken cancellationToken);

    Task UpdateActivityAsync(UpdateActivityDto activityDto, CancellationToken cancellationToken);

    Task DeleteActivityAsync(int id, CancellationToken cancellationToken);

    Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(CancellationToken cancellationToken);

    Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId, CancellationToken cancellationToken);
}
