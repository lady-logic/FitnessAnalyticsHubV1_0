namespace FitnessAnalyticsHub.Application.DTOs
{
    public class AIWorkoutAnalysisResponseDto
    {
        public string Analysis { get; set; } = string.Empty;
        public List<string> KeyInsights { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
