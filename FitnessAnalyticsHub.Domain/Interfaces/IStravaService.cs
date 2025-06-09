using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Models;

namespace FitnessAnalyticsHub.Domain.Interfaces;

public interface IStravaService
{
    Task<string> GetAuthorizationUrlAsync();
    Task<TokenInfo> ExchangeCodeForTokenAsync(string code);
    Task<Athlete> GetAthleteProfileAsync(string accessToken);
    Task<IEnumerable<Activity>> GetActivitiesAsync(string accessToken, int page = 1, int perPage = 30);
    Task<Activity> GetActivityDetailsByIdAsync(string accessToken, string activityId);
    Task<(Athlete athlete, IEnumerable<Activity> activities)> ImportMyActivitiesAsync();
}
