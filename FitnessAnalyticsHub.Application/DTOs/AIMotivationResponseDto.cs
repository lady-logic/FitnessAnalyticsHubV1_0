namespace FitnessAnalyticsHub.Application.DTOs
{
    public class AIMotivationResponseDto
    {
        public string MotivationalMessage { get; set; } = string.Empty;
        public string? Quote { get; set; }
        public List<string>? ActionableTips { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
