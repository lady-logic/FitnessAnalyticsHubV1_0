namespace FitnessAnalyticsHub.Application.Interfaces;

using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<Activity> Activities { get; }

    DbSet<Athlete> Athletes { get; }

    DbSet<TrainingPlan> TrainingPlans { get; }

    DbSet<PlannedActivity> PlannedActivities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}