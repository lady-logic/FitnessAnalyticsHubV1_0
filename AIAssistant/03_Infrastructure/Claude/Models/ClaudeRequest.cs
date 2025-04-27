namespace FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Claude
{
    public class ClaudeRequest
    {
        public string Model { get; set; }
        public List<object> Messages { get; set; }
        public double Temperature { get; set; }
        public int Max_tokens { get; set; } // Claude verwendet snake_case
    }
}
