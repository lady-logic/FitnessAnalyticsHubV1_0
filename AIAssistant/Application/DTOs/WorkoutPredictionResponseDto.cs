namespace AIAssistant.Application.DTOs;

public class WorkoutPredictionResponseDto
{
    public double PredictedDistance { get; set; }
    public int PredictedDuration { get; set; } // in seconds
    public double? PredictedPace { get; set; } // minutes per km
    public int? PredictedCalories { get; set; }
    public string? Explanation { get; set; }
    public List<string>? PreparationTips { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
