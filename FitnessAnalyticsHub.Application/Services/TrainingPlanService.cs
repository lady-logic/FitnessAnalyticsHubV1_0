using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Application.Services;

public class TrainingPlanService : ITrainingPlanService
{

    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TrainingPlanService(
        IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TrainingPlanDto?> GetTrainingPlanByIdAsync(int id, CancellationToken cancellationToken)
    {
        var trainingPlan = await _context.TrainingPlans
            .Include(tp => tp.Athlete)                           // Athlete für AthleteName
            .Include(tp => tp.PlannedActivities)                 // PlannedActivities
                .ThenInclude(pa => pa.CompletedActivity)         // CompletedActivity wenn vorhanden
                    .ThenInclude(ca => ca.Athlete)               // Athlete der CompletedActivity
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (trainingPlan == null)
            return null;

        return _mapper.Map<TrainingPlanDto>(trainingPlan);
    }

    public async Task<IEnumerable<TrainingPlanDto>> GetTrainingPlansByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        var trainingPlans = await _context.TrainingPlans
            .Include(tp => tp.Athlete)
            .Include(tp => tp.PlannedActivities)
                .ThenInclude(pa => pa.CompletedActivity)
                    .ThenInclude(ca => ca.Athlete)
            .Where(tp => tp.AthleteId == athleteId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<TrainingPlanDto>>(trainingPlans);
    }

    public async Task<TrainingPlanDto> CreateTrainingPlanAsync(CreateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        var trainingPlan = _mapper.Map<TrainingPlan>(trainingPlanDto);

        await _context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // TrainingPlan mit Athlete laden für das Mapping
        var trainingPlanWithAthlete = await _context.TrainingPlans
            .Include(tp => tp.Athlete)
            .FirstAsync(tp => tp.Id == trainingPlan.Id, cancellationToken);

        return _mapper.Map<TrainingPlanDto>(trainingPlanWithAthlete);
    }

    public async Task UpdateTrainingPlanAsync(UpdateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        var trainingPlan = await _context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == trainingPlanDto.Id, cancellationToken);

        if (trainingPlan == null)
            throw new Exception($"Training plan with ID {trainingPlanDto.Id} not found");

        _mapper.Map(trainingPlanDto, trainingPlan);
        trainingPlan.UpdatedAt = DateTime.Now;

        // Kein Update() nötig - EF Core tracked automatisch!
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTrainingPlanAsync(int id, CancellationToken cancellationToken)
    {
        var trainingPlan = await _context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

        if (trainingPlan == null)
            throw new Exception($"Training plan with ID {id} not found");

        _context.TrainingPlans.Remove(trainingPlan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> AddPlannedActivityAsync(int trainingPlanId, CreatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        var trainingPlanExists = await _context.TrainingPlans
            .AnyAsync(tp => tp.Id == trainingPlanId, cancellationToken);

        if (!trainingPlanExists)
            throw new Exception($"Training plan with ID {trainingPlanId} not found");

        var plannedActivity = _mapper.Map<PlannedActivity>(plannedActivityDto);
        plannedActivity.TrainingPlanId = trainingPlanId;

        await _context.PlannedActivities.AddAsync(plannedActivity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PlannedActivityDto>(plannedActivity);
    }

    public async Task UpdatePlannedActivityAsync(UpdatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        var plannedActivity = await _context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityDto.Id, cancellationToken);

        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityDto.Id} not found");

        _mapper.Map(plannedActivityDto, plannedActivity);

        // Kein Update() nötig - EF Core tracked automatisch!
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlannedActivityAsync(int plannedActivityId, CancellationToken cancellationToken)
    {
        var plannedActivity = await _context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");

        _context.PlannedActivities.Remove(plannedActivity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> MarkPlannedActivityAsCompletedAsync(int plannedActivityId, int activityId, CancellationToken cancellationToken)
    {
        var plannedActivity = await _context.PlannedActivities
            .FirstOrDefaultAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");

        var activityExists = await _context.Activities
            .AnyAsync(a => a.Id == activityId, cancellationToken);

        if (!activityExists)
            throw new Exception($"Activity with ID {activityId} not found");

        plannedActivity.CompletedActivityId = activityId;
        await _context.SaveChangesAsync(cancellationToken);

        // PlannedActivity mit CompletedActivity laden für das Mapping
        var plannedActivityWithActivity = await _context.PlannedActivities
            .Include(pa => pa.CompletedActivity)
                .ThenInclude(ca => ca.Athlete)
            .FirstAsync(pa => pa.Id == plannedActivityId, cancellationToken);

        return _mapper.Map<PlannedActivityDto>(plannedActivityWithActivity);
    }
}
