using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Application.DTOs
{
    public class PredictionResultDto
    {
        public double PredictedValue { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime PredictionDate { get; set; }
        public string SportType { get; set; } = string.Empty;
    }
}
