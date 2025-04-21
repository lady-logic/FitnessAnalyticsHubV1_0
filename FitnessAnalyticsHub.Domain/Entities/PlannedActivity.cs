using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Domain.Entities
{
    public class PlannedActivity
    {
        public int Id { get; set; }
        public int TrainingPlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SportType { get; set; } = string.Empty;
        public DateTime PlannedDate { get; set; }
        public int? PlannedDuration { get; set; }
        public double? PlannedDistance { get; set; }
        public int? CompletedActivityId { get; set; }

        public virtual TrainingPlan? TrainingPlan { get; set; }
        public virtual Activity? CompletedActivity { get; set; }
    }
}
