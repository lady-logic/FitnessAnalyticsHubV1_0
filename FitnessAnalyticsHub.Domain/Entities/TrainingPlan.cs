using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitnessAnalyticsHub.Domain.Enums;

namespace FitnessAnalyticsHub.Domain.Entities;

public class TrainingPlan
{
    [Required]
    public int Id { get; set; }
    public int AthleteId { get; set; }
    [ForeignKey("AthleteId")]
    public virtual Athlete? Athlete { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TrainingGoal Goal { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public virtual ICollection<PlannedActivity> PlannedActivities { get; set; } = new List<PlannedActivity>();
}
