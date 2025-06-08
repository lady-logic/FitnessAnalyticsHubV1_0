namespace FitnessAnalyticsHub.Domain.Models;

public class PredictionResult
{
    public double PredictedValue { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime PredictionDate { get; set; } = DateTime.Now;
}
