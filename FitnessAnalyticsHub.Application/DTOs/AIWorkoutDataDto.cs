namespace FitnessAnalyticsHub.Application.DTOs
{
    public class AIWorkoutDataDto
    {
        public DateTime Date { get; set; }

        public string ActivityType { get; set; } = string.Empty;

        public double Distance { get; set; }

        public int Duration { get; set; }

        public int Calories { get; set; }
    }
}
