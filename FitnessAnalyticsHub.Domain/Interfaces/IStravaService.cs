namespace FitnessAnalyticsHub.Domain.Interfaces;

using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Models;

public interface IStravaService
{
    Task<string> GetAuthorizationUrlAsync(CancellationToken cancellationToken);

    Task<TokenInfo> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken);

    Task<Athlete> GetAthleteProfileAsync(string accessToken, CancellationToken cancellationToken);

    Task<IEnumerable<Activity>> GetActivitiesAsync(string accessToken, CancellationToken cancellationToken, int page = 1, int perPage = 30);

    Task<Activity> GetActivityDetailsByIdAsync(string accessToken, string activityId, CancellationToken cancellationToken);

    Task<(Athlete athlete, IEnumerable<Activity> activities)> ImportMyActivitiesAsync(CancellationToken cancellationToken);
}
