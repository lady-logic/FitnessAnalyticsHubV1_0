namespace AIAssistant.Domain.Models;

public class WorkoutData
{
    public DateTime Date { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public double Distance { get; set; }

    public int Duration { get; set; } // in seconds

    public int? Calories { get; set; }

    public Dictionary<string, double>? MetricsData { get; set; } // Heart rate, pace, etc.
}
