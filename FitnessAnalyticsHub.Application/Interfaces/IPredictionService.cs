using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces
{
    public interface IPredictionService
    {
        Task<PredictionResultDto> PredictPerformanceAsync(int athleteId, string sportType);
        Task TrainModelAsync(int athleteId);
        Task<bool> IsModelTrainedForAthleteAsync(int athleteId);
        Task<ModelEvaluationDto> EvaluateModelAsync(int athleteId);
    }
}
