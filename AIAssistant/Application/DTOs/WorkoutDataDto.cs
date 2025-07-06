using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

public class WorkoutDataDto  
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public double Distance { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Duration { get; set; }

    [Range(0, int.MaxValue)]
    public int? Calories { get; set; }

    public Dictionary<string, double>? MetricsData { get; set; }
}
