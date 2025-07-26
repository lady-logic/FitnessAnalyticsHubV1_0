using System.ComponentModel.DataAnnotations;
using FitnessAnalyticsHub.Domain.Enums;

namespace FitnessAnalyticsHub.Application.DTOs;

public class TrainingPlanDto
{
    public int Id { get; set; }
    [Required]
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TrainingGoal Goal { get; set; }
    public string? Notes { get; set; }
    public List<PlannedActivityDto> PlannedActivities { get; set; } = new List<PlannedActivityDto>();
}
