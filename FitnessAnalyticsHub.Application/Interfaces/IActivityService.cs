using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces
{
    public interface IActivityService
    {
        Task<ActivityDto?> GetActivityByIdAsync(int id);
        Task<IEnumerable<ActivityDto>> GetActivitiesByAthleteIdAsync(int athleteId);
        Task<ActivityDto> CreateActivityAsync(CreateActivityDto activityDto);
        Task UpdateActivityAsync(UpdateActivityDto activityDto);
        Task DeleteActivityAsync(int id);
        Task<IEnumerable<ActivityDto>> ImportActivitiesFromStravaAsync(int athleteId, string accessToken);
        Task<ActivityStatisticsDto> GetAthleteActivityStatisticsAsync(int athleteId);
    }
}
