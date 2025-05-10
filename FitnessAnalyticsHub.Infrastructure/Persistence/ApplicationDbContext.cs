using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessAnalyticsHub.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
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

            // Athlete configuration
            modelBuilder.Entity<Athlete>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StravaId).HasMaxLength(50);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);

                entity.HasMany(e => e.Activities)
                      .WithOne(e => e.Athlete)
                      .HasForeignKey(e => e.AthleteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TrainingPlans)
                      .WithOne(e => e.Athlete)
                      .HasForeignKey(e => e.AthleteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Activity configuration
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.StravaId).HasMaxLength(50);
                entity.Property(e => e.SportType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Timezone).HasMaxLength(50);
                entity.OwnsOne(a => a.Pace, paceBuilder =>
                {
                    paceBuilder.Property(p => p.ValuePerKilometer)
                        .HasColumnName("PacePerKilometerInTicks");
                });

                entity.HasOne(e => e.Athlete)
                      .WithMany(e => e.Activities)
                      .HasForeignKey(e => e.AthleteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        }
}
