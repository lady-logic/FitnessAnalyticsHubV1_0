using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Domain.Entities
{
    public class TrainingPlan
    {
        public int Id { get; set; }
        public int AthleteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TrainingGoal Goal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Athlete? Athlete { get; set; }
        public virtual ICollection<PlannedActivity> PlannedActivities { get; set; } = new List<PlannedActivity>();
    }
}
