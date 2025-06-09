namespace FitnessAnalyticsHub.Application.DTOs;

public class ActivityStatisticsDto
{
    public int TotalActivities { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double TotalElevationGain { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new Dictionary<string, int>();
    public Dictionary<int, int> ActivitiesByMonth { get; set; } = new Dictionary<int, int>();
}
