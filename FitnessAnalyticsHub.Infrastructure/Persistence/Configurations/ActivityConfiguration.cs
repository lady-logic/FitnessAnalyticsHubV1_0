using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessAnalyticsHub.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.StravaId)
            .HasMaxLength(50);

        builder.Property(e => e.SportType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Timezone)
            .HasMaxLength(50);

        // Value Object Configuration
        builder.OwnsOne(a => a.Pace, paceBuilder =>
        {
            paceBuilder.Property(p => p.ValuePerKilometer)
                .HasColumnName("PacePerKilometerInTicks");
        });

        // Relationships
        builder.HasOne(e => e.Athlete)
            .WithMany(e => e.Activities)
            .HasForeignKey(e => e.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}