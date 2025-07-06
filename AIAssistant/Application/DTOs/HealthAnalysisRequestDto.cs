using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AIAssistant.Application.DTOs;

public class HealthAnalysisRequestDto
{
    [Required]
    public int AthleteId { get; set; }
    public List<WorkoutDataDto> RecentWorkouts { get; set; } = new();

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