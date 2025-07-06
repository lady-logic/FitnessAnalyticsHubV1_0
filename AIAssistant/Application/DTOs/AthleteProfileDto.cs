using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

public class AthleteProfileDto
{
    public string? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? FitnessLevel { get; set; }

    [StringLength(100)]
    public string? PrimaryGoal { get; set; }

    public Dictionary<string, object>? Preferences { get; set; }
}
