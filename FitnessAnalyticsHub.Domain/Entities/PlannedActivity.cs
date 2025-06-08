using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAnalyticsHub.Domain.Entities;

public class PlannedActivity
{
    [Required]
    public int Id { get; set; }
    public int TrainingPlanId { get; set; }
    [ForeignKey("TrainingPlanId")]
    public virtual TrainingPlan? TrainingPlan { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SportType { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public int? PlannedDuration { get; set; }
    public double? PlannedDistance { get; set; }
    public int? CompletedActivityId { get; set; }
    [ForeignKey("CompletedActivityId")]
    public virtual Activity? CompletedActivity { get; set; }
}
