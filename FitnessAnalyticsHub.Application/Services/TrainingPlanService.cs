using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;

namespace FitnessAnalyticsHub.Application.Services;

public class TrainingPlanService : ITrainingPlanService
{

    private readonly IRepository<TrainingPlan> _trainingPlanRepository;
    private readonly IRepository<PlannedActivity> _plannedActivityRepository;
    private readonly IRepository<Activity> _activityRepository;
    private readonly IRepository<Athlete> _athleteRepository;
    private readonly IMapper _mapper;

    public TrainingPlanService(
    IRepository<TrainingPlan> trainingPlanRepository,
    IRepository<PlannedActivity> plannedActivityRepository,
    IRepository<Activity> activityRepository,
    IRepository<Athlete> athleteRepository,
    IMapper mapper)
    {
        _trainingPlanRepository = trainingPlanRepository;
        _plannedActivityRepository = plannedActivityRepository;
        _activityRepository = activityRepository;
        _athleteRepository = athleteRepository;
        _mapper = mapper;
    }

    public async Task<TrainingPlanDto?> GetTrainingPlanByIdAsync(int id, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanRepository.GetByIdAsync(id, cancellationToken);
        if (trainingPlan == null)
            return null;

        var trainingPlanDto = _mapper.Map<TrainingPlanDto>(trainingPlan);

        // Get planned activities
        var plannedActivities = await _plannedActivityRepository.FindAsync(pa => pa.TrainingPlanId == id, cancellationToken);
        var plannedActivityDtos = new List<PlannedActivityDto>();

        foreach (var plannedActivity in plannedActivities)
        {
            var plannedActivityDto = _mapper.Map<PlannedActivityDto>(plannedActivity);

            if (plannedActivity.CompletedActivityId.HasValue)
            {
                var completedActivity = await _activityRepository.GetByIdAsync(plannedActivity.CompletedActivityId.Value, cancellationToken);
                if (completedActivity != null)
                {
                    plannedActivityDto.CompletedActivity = _mapper.Map<ActivityDto>(completedActivity);
                }
            }

            plannedActivityDtos.Add(plannedActivityDto);
        }

        trainingPlanDto.PlannedActivities = plannedActivityDtos;

        // Get athlete name
        var athlete = await _athleteRepository.GetByIdAsync(trainingPlan.AthleteId, cancellationToken);
        if (athlete != null)
        {
            trainingPlanDto.AthleteName = $"{athlete.FirstName} {athlete.LastName}";
        }

        return trainingPlanDto;
    }

    public async Task<IEnumerable<TrainingPlanDto>> GetTrainingPlansByAthleteIdAsync(int athleteId, CancellationToken cancellationToken)
    {
        var trainingPlans = await _trainingPlanRepository.FindAsync(tp => tp.AthleteId == athleteId, cancellationToken);

        var athlete = await _athleteRepository.GetByIdAsync(athleteId, cancellationToken);
        var athleteName = athlete != null ? $"{athlete.FirstName} {athlete.LastName}" : string.Empty;

        var trainingPlanDtos = _mapper.Map<IEnumerable<TrainingPlanDto>>(trainingPlans).ToList();

        foreach (var trainingPlanDto in trainingPlanDtos)
        {
            trainingPlanDto.AthleteName = athleteName;

            // Get planned activities
            var plannedActivities = await _plannedActivityRepository.FindAsync(pa => pa.TrainingPlanId == trainingPlanDto.Id, cancellationToken);
            var plannedActivityDtos = new List<PlannedActivityDto>();

            foreach (var plannedActivity in plannedActivities)
            {
                var plannedActivityDto = _mapper.Map<PlannedActivityDto>(plannedActivity);

                if (plannedActivity.CompletedActivityId.HasValue)
                {
                    var completedActivity = await _activityRepository.GetByIdAsync(plannedActivity.CompletedActivityId.Value, cancellationToken);
                    if (completedActivity != null)
                    {
                        plannedActivityDto.CompletedActivity = _mapper.Map<ActivityDto>(completedActivity);
                    }
                }

                plannedActivityDtos.Add(plannedActivityDto);
            }

            trainingPlanDto.PlannedActivities = plannedActivityDtos;
        }

        return trainingPlanDtos;
    }

    public async Task<TrainingPlanDto> CreateTrainingPlanAsync(CreateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        var trainingPlan = _mapper.Map<TrainingPlan>(trainingPlanDto);

        await _trainingPlanRepository.AddAsync(trainingPlan, cancellationToken);
        await _trainingPlanRepository.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<TrainingPlanDto>(trainingPlan);

        var athlete = await _athleteRepository.GetByIdAsync(trainingPlan.AthleteId, cancellationToken);
        if (athlete != null)
        {
            resultDto.AthleteName = $"{athlete.FirstName} {athlete.LastName}";
        }

        return resultDto;
    }

    public async Task UpdateTrainingPlanAsync(UpdateTrainingPlanDto trainingPlanDto, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanRepository.GetByIdAsync(trainingPlanDto.Id, cancellationToken);
        if (trainingPlan == null)
            throw new Exception($"Training plan with ID {trainingPlanDto.Id} not found");

        _mapper.Map(trainingPlanDto, trainingPlan);
        trainingPlan.UpdatedAt = DateTime.Now;

        await _trainingPlanRepository.UpdateAsync(trainingPlan);
        await _trainingPlanRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTrainingPlanAsync(int id, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanRepository.GetByIdAsync(id, cancellationToken);
        if (trainingPlan == null)
            throw new Exception($"Training plan with ID {id} not found");

        await _trainingPlanRepository.DeleteAsync(trainingPlan);
        await _trainingPlanRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> AddPlannedActivityAsync(int trainingPlanId, CreatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        var trainingPlan = await _trainingPlanRepository.GetByIdAsync(trainingPlanId, cancellationToken);
        if (trainingPlan == null)
            throw new Exception($"Training plan with ID {trainingPlanId} not found");

        var plannedActivity = _mapper.Map<PlannedActivity>(plannedActivityDto);
        plannedActivity.TrainingPlanId = trainingPlanId;

        await _plannedActivityRepository.AddAsync(plannedActivity, cancellationToken);
        await _plannedActivityRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PlannedActivityDto>(plannedActivity);
    }

    public async Task UpdatePlannedActivityAsync(UpdatePlannedActivityDto plannedActivityDto, CancellationToken cancellationToken)
    {
        var plannedActivity = await _plannedActivityRepository.GetByIdAsync(plannedActivityDto.Id, cancellationToken);
        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityDto.Id} not found");

        _mapper.Map(plannedActivityDto, plannedActivity);

        await _plannedActivityRepository.UpdateAsync(plannedActivity);
        await _plannedActivityRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlannedActivityAsync(int plannedActivityId, CancellationToken cancellationToken)
    {
        var plannedActivity = await _plannedActivityRepository.GetByIdAsync(plannedActivityId, cancellationToken);
        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");

        await _plannedActivityRepository.DeleteAsync(plannedActivity);
        await _plannedActivityRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlannedActivityDto> MarkPlannedActivityAsCompletedAsync(int plannedActivityId, int activityId, CancellationToken cancellationToken)
    {
        var plannedActivity = await _plannedActivityRepository.GetByIdAsync(plannedActivityId, cancellationToken);
        if (plannedActivity == null)
            throw new Exception($"Planned activity with ID {plannedActivityId} not found");

        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null)
            throw new Exception($"Activity with ID {activityId} not found");

        plannedActivity.CompletedActivityId = activityId;

        await _plannedActivityRepository.UpdateAsync(plannedActivity);
        await _plannedActivityRepository.SaveChangesAsync(cancellationToken);

        var plannedActivityDto = _mapper.Map<PlannedActivityDto>(plannedActivity);
        plannedActivityDto.CompletedActivity = _mapper.Map<ActivityDto>(activity);

        return plannedActivityDto;
    }
}
