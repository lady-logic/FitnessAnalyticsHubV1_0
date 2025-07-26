using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitnessAnalyticsHub.Domain.ValueObjects;

namespace FitnessAnalyticsHub.Domain.Entities;

public class Activity
{
    [Required]
    public int Id { get; set; }

    public string? StravaId { get; set; }

    public int AthleteId { get; set; }

    [ForeignKey(nameof(AthleteId))]
    public virtual Athlete? Athlete { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Distance { get; set; }
    public int MovingTime { get; set; }
    public int ElapsedTime { get; set; }
    public double TotalElevationGain { get; set; }
    public string SportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime StartDateLocal { get; set; }
    public string? Timezone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public double? AverageSpeed { get; set; }
    public double? MaxSpeed { get; set; }
    public int? AverageHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }
    public double? AveragePower { get; set; }
    public double? MaxPower { get; set; }
    public double? AverageCadence { get; set; }
    public Pace? Pace { get; private set; }

    // Helper-Methode zum Setzen der Pace
    public void SetPace(double distance, TimeSpan duration)
    {
        if (distance > 0 && duration > TimeSpan.Zero)
        {
            try
            {
                var distanceInKm = distance / 1000.0; // Strava gibt Meter zurück
                this.Pace = Pace.FromDistanceAndDuration(distanceInKm, duration);
            }
            catch (ArgumentException)
            {
                this.Pace = null; // Falls Pace ungültig ist
            }
        }
        else
        {
            this.Pace = null; // Keine gültige Distanz/Zeit
        }
    }
}
