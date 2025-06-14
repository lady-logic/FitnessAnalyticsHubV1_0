public class AIAssistantMotivationRequest
{
    public AIAssistantAthleteProfile AthleteProfile { get; set; } = new();
    public List<AIAssistantWorkout> RecentWorkouts { get; set; } = new();
    public string PreferredTone { get; set; } = string.Empty;
    public string ContextualInfo { get; set; } = string.Empty;
}