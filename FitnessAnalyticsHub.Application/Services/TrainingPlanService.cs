namespace FitnessAnalyticsHub.Application.Services;

using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class TrainingPlanService : ITrainingPlanService
{
    private readonly IApplicationDbContext context;
    private readonly IMapper mapper;

    public TrainingPlanService(
        IApplicationDbContext context,
        IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task<TrainingPlanDto?> GetTrainingPlanByIdAsync(int id, CancellationToken cancellationToken)
    {
        TrainingPlan? trainingPlan = await this.context.TrainingPlans
            .Include(tp => tp.Athlete) // Athlete für AthleteName
            .Include(tp => tp.PlannedActivities) // PlannedActivities
                .ThenInclude(pa => pa.CompletedActivity) // CompletedActivity wenn vorhanden
                    .ThenInclude(ca => ca.Athlete) // Athlete der CompletedActivity
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (trainingPlan == null)
        {
            return null;
        }

        return this.mapper.Map<TrainingPlanDto>(trainingPlan);
    }

    public async Task<IEnumerable<TrainingPlanDto>> GetTrainingPlansByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        List<TrainingPlan> trainingPlans = await this.context.TrainingPlans
            .Include(tp => tp.Athlete)
            .Include(tp => tp.PlannedActivities)
                .ThenInclude(pa => pa.CompletedActivity)
                    .ThenInclude(ca => ca.Athlete)
            .Where(tp => tp.AthleteId == athleteId)
            .ToListAsync(cancellationToken);

        return this.mapper.Map<IEnumerable<TrainingPlanDto>>(trainingPlans);
    }

    public async Task<TrainingPlanDto> CreateTrainingPlanAsync(CreateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        TrainingPlan trainingPlan = this.mapper.Map<TrainingPlan>(trainingPlanDto);

        await this.context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
        await this.context.SaveChangesAsync(cancellationToken);

        // TrainingPlan mit Athlete laden für das Mapping
        TrainingPlan trainingPlanWithAthlete = await this.context.TrainingPlans
            .Include(tp => tp.Athlete)
            .FirstAsync(tp => tp.Id == trainingPlan.Id, cancellationToken);

        return this.mapper.Map<TrainingPlanDto>(trainingPlanWithAthlete);
    }

    public async Task UpdateTrainingPlanAsync(UpdateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        TrainingPlan? trainingPlan = await this.context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == trainingPlanDto.Id, cancellationToken);

        if (trainingPlan == null)
        {
            throw new Exception($"Training plan with ID {trainingPlanDto.Id} not found");
        }

        this.mapper.Map(trainingPlanDto, trainingPlan);
        trainingPlan.UpdatedAt = DateTime.Now;

        // Kein Update() nötig - EF Core tracked automatisch!
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTrainingPlanAsync(int id, CancellationToken cancellationToken)
    {
        TrainingPlan? trainingPlan = await this.context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (trainingPlan == null)
        {
            throw new Exception($"Training plan with ID {id} not found");
        }

        this.context.TrainingPlans.Remove(trainingPlan);
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> AddPlannedActivityAsync(int trainingPlanId, CreatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        bool trainingPlanExists = await this.context.TrainingPlans
            .AnyAsync(tp => tp.Id == trainingPlanId, cancellationToken);

        if (!trainingPlanExists)
        {
            throw new Exception($"Training plan with ID {trainingPlanId} not found");
        }

        PlannedActivity plannedActivity = this.mapper.Map<PlannedActivity>(plannedActivityDto);
        plannedActivity.TrainingPlanId = trainingPlanId;

        await this.context.PlannedActivities.AddAsync(plannedActivity, cancellationToken);
        await this.context.SaveChangesAsync(cancellationToken);

        return this.mapper.Map<PlannedActivityDto>(plannedActivity);
    }

    public async Task UpdatePlannedActivityAsync(UpdatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        PlannedActivity? plannedActivity = await this.context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityDto.Id, cancellationToken);

        if (plannedActivity == null)
        {
            throw new Exception($"Planned activity with ID {plannedActivityDto.Id} not found");
        }

        this.mapper.Map(plannedActivityDto, plannedActivity);

        // Kein Update() nötig - EF Core tracked automatisch!
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlannedActivityAsync(int plannedActivityId, CancellationToken cancellationToken)
    {
        PlannedActivity? plannedActivity = await this.context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        if (plannedActivity == null)
        {
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");
        }

        this.context.PlannedActivities.Remove(plannedActivity);
        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> MarkPlannedActivityAsCompletedAsync(int plannedActivityId, int activityId, CancellationToken cancellationToken)
    {
        PlannedActivity? plannedActivity = await this.context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        if (plannedActivity == null)
        {
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");
        }

        bool activityExists = await this.context.Activities
            .AnyAsync(a => a.Id == activityId, cancellationToken);

        if (!activityExists)
        {
            throw new Exception($"Activity with ID {activityId} not found");
        }

        plannedActivity.CompletedActivityId = activityId;
        await this.context.SaveChangesAsync(cancellationToken);

        // PlannedActivity mit CompletedActivity laden für das Mapping
        PlannedActivity plannedActivityWithActivity = await this.context.PlannedActivities
            .Include(pa => pa.CompletedActivity)
                .ThenInclude(ca => ca.Athlete)
            .FirstAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        return this.mapper.Map<PlannedActivityDto>(plannedActivityWithActivity);
    }
}
