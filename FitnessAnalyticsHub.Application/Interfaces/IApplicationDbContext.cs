using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Activity> Activities { get; }
    DbSet<Athlete> Athletes { get; }
    DbSet<TrainingPlan> TrainingPlans { get; }
    DbSet<PlannedActivity> PlannedActivities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}