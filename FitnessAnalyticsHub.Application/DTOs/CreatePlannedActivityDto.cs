﻿namespace FitnessAnalyticsHub.Application;

public class CreatePlannedActivityDto
{
    public int TrainingPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string SportType { get; set; } = string.Empty;

    public DateTime PlannedDate { get; set; }

    public int? PlannedDurationMinutes { get; set; }

    public double? PlannedDistance { get; set; }
}
