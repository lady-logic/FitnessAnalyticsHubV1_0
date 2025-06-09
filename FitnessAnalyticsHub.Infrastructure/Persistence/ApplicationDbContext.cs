using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<Athlete> Athletes { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<TrainingPlan> TrainingPlans { get; set; }
    public DbSet<PlannedActivity> PlannedActivities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
