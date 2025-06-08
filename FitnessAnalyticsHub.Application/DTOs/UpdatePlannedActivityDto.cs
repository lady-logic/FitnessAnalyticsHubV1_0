namespace FitnessAnalyticsHub.Application.DTOs;

public class UpdatePlannedActivityDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SportType { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public int? PlannedDurationMinutes { get; set; }
    public double? PlannedDistance { get; set; }
}
