namespace AIAssistant.Domain.Models
{
    public class AthleteProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FitnessLevel { get; set; }
        public string? PrimaryGoal { get; set; }
        public Dictionary<string, object>? Preferences { get; set; }
    }
}
