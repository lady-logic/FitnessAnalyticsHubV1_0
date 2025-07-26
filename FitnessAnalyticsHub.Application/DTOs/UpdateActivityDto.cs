namespace FitnessAnalyticsHub.Application.DTOs;

public class UpdateActivityDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Distance { get; set; }

    public int MovingTimeSeconds { get; set; }

    public int ElapsedTimeSeconds { get; set; }

    public double TotalElevationGain { get; set; }

    public string SportType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime StartDateLocal { get; set; }

    public double? AverageSpeed { get; set; }

    public double? MaxSpeed { get; set; }

    public int? AverageHeartRate { get; set; }

    public int? MaxHeartRate { get; set; }

    public double? AveragePower { get; set; }

    public double? MaxPower { get; set; }

    public double? AverageCadence { get; set; }
}
