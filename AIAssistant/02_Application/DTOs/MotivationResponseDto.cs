namespace AIAssistant._02_Application.DTOs
{
    public class MotivationResponseDto
    {
        public string MotivationalMessage { get; set; } = string.Empty;
        public string? Quote { get; set; }
        public List<string>? ActionableTips { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
