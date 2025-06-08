using FitnessAnalyticsHub.Domain.Enums;

namespace FitnessAnalyticsHub.Application.DTOs;

public class CreateTrainingPlanDto
{
    public int AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TrainingGoal Goal { get; set; }
    public string? Notes { get; set; }
}
