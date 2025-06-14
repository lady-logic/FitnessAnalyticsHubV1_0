namespace FitnessAnalyticsHub.Application.DTOs
{
    public class AIWorkoutAnalysisRequestDto
    {
        public AIAthleteProfileDto? AthleteProfile { get; set; }
        public List<AIWorkoutDataDto>? RecentWorkouts { get; set; }
        public string? AnalysisType { get; set; }
        public List<string>? FocusAreas { get; set; }
    }
}
