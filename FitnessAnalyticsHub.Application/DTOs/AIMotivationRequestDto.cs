namespace FitnessAnalyticsHub.Application.DTOs
{
    public class AIMotivationRequestDto
    {
        public AIAthleteProfileDto AthleteProfile { get; set; } = new();

        public List<AIWorkoutDataDto>? RecentWorkouts { get; set; }

        public string? PreferredTone { get; set; }

        public string? ContextualInfo { get; set; }
    }
}
