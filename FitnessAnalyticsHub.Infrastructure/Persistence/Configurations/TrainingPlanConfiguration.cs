using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessAnalyticsHub.Infrastructure.Persistence.Configurations;

public class TrainingPlanConfiguration : IEntityTypeConfiguration<TrainingPlan>
{
    public void Configure(EntityTypeBuilder<TrainingPlan> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Athlete)
            .WithMany(e => e.TrainingPlans)
            .HasForeignKey(e => e.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PlannedActivities)
            .WithOne(e => e.TrainingPlan)
            .HasForeignKey(e => e.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}