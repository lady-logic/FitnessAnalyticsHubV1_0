using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces;

public interface ITrainingPlanService
{
    Task<TrainingPlanDto?> GetTrainingPlanByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<TrainingPlanDto>> GetTrainingPlansByAthleteIdAsync(int athleteId, CancellationToken cancellationToken);
    Task<TrainingPlanDto> CreateTrainingPlanAsync(CreateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken);
    Task UpdateTrainingPlanAsync(UpdateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken);
    Task DeleteTrainingPlanAsync(int id, CancellationToken cancellationToken);
    Task<PlannedActivityDto> AddPlannedActivityAsync(int trainingPlanId, CreatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken);
    Task UpdatePlannedActivityAsync(UpdatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken);
    Task DeletePlannedActivityAsync(int plannedActivityId, CancellationToken cancellationToken);
    Task<PlannedActivityDto> MarkPlannedActivityAsCompletedAsync(int plannedActivityId, int activityId, CancellationToken cancellationToken);
}
