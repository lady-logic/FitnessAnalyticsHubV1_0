using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Application.DTOs
{
    public class ModelEvaluationDto
    {
        public double RSquared { get; set; }
        public double MeanAbsoluteError { get; set; }
        public double RootMeanSquaredError { get; set; }
        public int DataPointsCount { get; set; }
    }
}
