﻿using System.Text.Json.Serialization;

namespace FitnessAnalyticsHub.Application.DTOs;

public class CreateActivityDto
{
    public string? StravaId { get; set; }
    public int AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Distance { get; set; }
    public int MovingTimeSeconds { get; set; }
    public int ElapsedTimeSeconds { get; set; }
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

    [JsonIgnore] // Wird nur für die Antwort berechnet, nicht im Request erwartet
    public string? CalculatedPace => Distance > 0 && MovingTimeSeconds > 0
        ? $"{TimeSpan.FromSeconds(MovingTimeSeconds / Distance * 1000).Minutes}:{TimeSpan.FromSeconds(MovingTimeSeconds / Distance * 1000).Seconds:D2} min/km"
        : null;
}
