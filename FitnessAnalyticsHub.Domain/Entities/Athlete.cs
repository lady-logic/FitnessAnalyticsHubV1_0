using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Domain.Entities
{
    public class Athlete
    {
        public int Id { get; set; }
        public string? StravaId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
    }
}
