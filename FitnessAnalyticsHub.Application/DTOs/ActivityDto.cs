using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.Application.DTOs;

public class ActivityDto
{
    public int Id { get; set; }

    public string? StravaId { get; set; }

    [Required]
    public int AthleteId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Distance { get; set; }

    public TimeSpan MovingTime { get; set; }

    public TimeSpan ElapsedTime { get; set; }

    public double TotalElevationGain { get; set; }

    public string SportType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime StartDateLocal { get; set; }

    public string? Timezone { get; set; }

    public double? AverageSpeed { get; set; }

    public double? MaxSpeed { get; set; }

    public int? AverageHeartRate { get; set; }

    public int? MaxHeartRate { get; set; }

    public double? AveragePower { get; set; }

    public double? MaxPower { get; set; }

    public double? AverageCadence { get; set; }

    public string AthleteFullName { get; set; } = string.Empty;
}
