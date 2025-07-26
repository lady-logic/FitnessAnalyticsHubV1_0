public class AIAssistantWorkoutAnalysisRequest
{
    public AIAssistantAthleteProfile? AthleteProfile { get; set; }
    public List<AIAssistantWorkout> RecentWorkouts { get; set; } = new ();
    public string AnalysisType { get; set; } = string.Empty;
    public List<string> FocusAreas { get; set; } = new ();
}