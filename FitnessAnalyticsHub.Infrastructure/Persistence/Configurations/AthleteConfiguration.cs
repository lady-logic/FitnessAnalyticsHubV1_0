namespace FitnessAnalyticsHub.Infrastructure.Persistence.Configurations;

using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AthleteConfiguration : IEntityTypeConfiguration<Athlete>
{
    public void Configure(EntityTypeBuilder<Athlete> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.StravaId)
            .HasMaxLength(50);

        builder.Property(e => e.Username)
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.Country)
            .HasMaxLength(100);

        builder.Property(e => e.ProfilePictureUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasMany(e => e.Activities)
            .WithOne(e => e.Athlete)
            .HasForeignKey(e => e.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.TrainingPlans)
            .WithOne(e => e.Athlete)
            .HasForeignKey(e => e.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}