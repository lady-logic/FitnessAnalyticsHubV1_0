﻿namespace AIAssistant._02_Application.DTOs;

public class HealthAnalysisRequestDto
{
    public int AthleteId { get; set; }
    public List<Domain.Models.WorkoutData> RecentWorkouts { get; set; } = new();

    /// <summary>
    /// Zusätzliche Gesundheitsmetriken (optional)
    /// </summary>
    public Dictionary<string, object>? HealthMetrics { get; set; }

    /// <summary>
    /// Spezifische Bereiche für die Analyse (z.B. "injury_prevention", "recovery", "overtraining")
    /// </summary>
    public List<string>? FocusAreas { get; set; }

    /// <summary>
    /// Bekannte Verletzungen oder gesundheitliche Einschränkungen
    /// </summary>
    public List<string>? KnownIssues { get; set; }
}