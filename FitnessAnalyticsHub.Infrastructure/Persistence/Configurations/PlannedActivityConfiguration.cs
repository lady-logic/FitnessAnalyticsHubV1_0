namespace FitnessAnalyticsHub.Infrastructure.Persistence.Configurations;

using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlannedActivityConfiguration : IEntityTypeConfiguration<PlannedActivity>
{
    public void Configure(EntityTypeBuilder<PlannedActivity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired();

        builder.Property(e => e.SportType)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.TrainingPlan)
            .WithMany(e => e.PlannedActivities)
            .HasForeignKey(e => e.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CompletedActivity)
            .WithMany()
            .HasForeignKey(e => e.CompletedActivityId);
    }
}