namespace FitnessAnalyticsHub.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using FitnessAnalyticsHub.Domain.Enums;

public class CreateTrainingPlanDto
{
    [Required]
    public int AthleteId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TrainingGoal Goal { get; set; }

    public string? Notes { get; set; }
}
