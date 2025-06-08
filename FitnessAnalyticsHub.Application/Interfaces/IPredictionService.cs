using FitnessAnalyticsHub.Application.DTOs;

namespace FitnessAnalyticsHub.Application.Interfaces;

public interface IPredictionService
{
    Task<PredictionResultDto> PredictPerformanceAsync(int athleteId, string sportType, CancellationToken cancellationToken);
    Task TrainModelAsync(int athleteId);
    Task<bool> IsModelTrainedForAthleteAsync(int athleteId, CancellationToken cancellationToken);
    Task<ModelEvaluationDto> EvaluateModelAsync(int athleteId);
}
