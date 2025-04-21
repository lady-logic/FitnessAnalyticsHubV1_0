using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Domain.Entities
{
    public class Activity
    {
        public int Id { get; set; }
        public string? StravaId { get; set; }
        public int AthleteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Distance { get; set; }
        public int MovingTime { get; set; }
        public int ElapsedTime { get; set; }
        public double TotalElevationGain { get; set; }
        public string SportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime StartDateLocal { get; set; }
        public string? Timezone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public double? AverageSpeed { get; set; }
        public double? MaxSpeed { get; set; }
        public int? AverageHeartRate { get; set; }
        public int? MaxHeartRate { get; set; }
        public double? AveragePower { get; set; }
        public double? MaxPower { get; set; }
        public double? AverageCadence { get; set; }

        public virtual Athlete? Athlete { get; set; }
    }
}
