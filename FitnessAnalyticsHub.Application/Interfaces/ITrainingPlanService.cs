using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces
{
    public interface ITrainingPlanService
    {
        Task<TrainingPlanDto?> GetTrainingPlanByIdAsync(int id);
        Task<IEnumerable<TrainingPlanDto>> GetTrainingPlansByAthleteIdAsync(int athleteId);
        Task<TrainingPlanDto> CreateTrainingPlanAsync(CreateTrainingPlanDto trainingPlanDto);
        Task UpdateTrainingPlanAsync(UpdateTrainingPlanDto trainingPlanDto);
        Task DeleteTrainingPlanAsync(int id);
        Task<PlannedActivityDto> AddPlannedActivityAsync(int trainingPlanId, CreatePlannedActivityDto plannedActivityDto);
        Task UpdatePlannedActivityAsync(UpdatePlannedActivityDto plannedActivityDto);
        Task DeletePlannedActivityAsync(int plannedActivityId);
        Task<PlannedActivityDto> MarkPlannedActivityAsCompletedAsync(int plannedActivityId, int activityId);
    }
}
