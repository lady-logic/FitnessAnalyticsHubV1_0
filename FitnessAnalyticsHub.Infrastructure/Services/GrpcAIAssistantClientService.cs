using Fitnessanalyticshub;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitnessAnalyticsHub.Infrastructure.Services;

public class GrpcAIAssistantClientService : IAIAssistantClientService, IDisposable
{
    private readonly GrpcChannel channel;
    private readonly ILogger<GrpcAIAssistantClientService> logger;
    private readonly MotivationService.MotivationServiceClient motivationClient;
    private readonly WorkoutService.WorkoutServiceClient workoutServiceClient;

    public GrpcAIAssistantClientService(
        ILogger<GrpcAIAssistantClientService> logger,
        IConfiguration configuration)
    {
        this.logger = logger;

        // gRPC Channel erstellen (wie HttpClient, aber für gRPC)
        var grpcUrl = configuration["AIAssistant:GrpcUrl"] ?? "http://localhost:5001";
        this.channel = GrpcChannel.ForAddress(grpcUrl);

        // Client erstellen!
        this.motivationClient = new MotivationService.MotivationServiceClient(this.channel);
        this.workoutServiceClient = new WorkoutService.WorkoutServiceClient(this.channel);

        this.logger.LogInformation("gRPC Channel created for: {GrpcUrl}", grpcUrl);
    }

    public async Task<AIMotivationResponseDto> GetMotivationAsync(AIMotivationRequestDto request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation(
            "gRPC: Requesting motivation for athlete: {AthleteName}",
            request.AthleteProfile?.Name ?? "Unknown");

        try
        {
            var grpcRequest = new MotivationRequest
            {
                AthleteProfile = new AthleteProfile
                {
                    Name = request.AthleteProfile?.Name ?? string.Empty,
                    FitnessLevel = request.AthleteProfile?.FitnessLevel ?? string.Empty,
                    PrimaryGoal = request.AthleteProfile?.PrimaryGoal ?? string.Empty,
                },
                PreferredTone = request.PreferredTone ?? string.Empty,
                ContextualInfo = request.ContextualInfo ?? string.Empty,
            };

            // gRPC-Call
            var grpcResponse = await this.motivationClient.GetMotivationAsync(grpcRequest, cancellationToken: cancellationToken);

            // gRPC-Response zu DTO konvertieren
            var response = new AIMotivationResponseDto
            {
                MotivationalMessage = grpcResponse.MotivationalMessage,
                Quote = grpcResponse.Quote,
                ActionableTips = grpcResponse.ActionableTips.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,
            };

            this.logger.LogInformation("gRPC: Motivation response received successfully");
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error getting motivation");
            throw;
        }
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation(
            "gRPC: Requesting workout analysis for {WorkoutCount} workouts",
            request.RecentWorkouts?.Count ?? 0);

        try
        {
            // Konvertiere DTO zu gRPC Request
            var grpcRequest = new WorkoutAnalysisRequest
            {
                AnalysisType = request.AnalysisType ?? "General",

                // PreferredAiProvider = request.PreferredAiProvider ?? "huggingface"
                PreferredAiProvider = "googlegemini",
            };

            // AthleteProfile hinzufügen
            if (request.AthleteProfile != null)
            {
                grpcRequest.AthleteProfile = new AthleteProfile
                {
                    Name = request.AthleteProfile.Name ?? string.Empty,
                    FitnessLevel = request.AthleteProfile.FitnessLevel ?? string.Empty,
                    PrimaryGoal = request.AthleteProfile.PrimaryGoal ?? string.Empty,
                };
            }

            // Workouts hinzufügen
            if (request.RecentWorkouts != null)
            {
                foreach (var workout in request.RecentWorkouts)
                {
                    var grpcWorkout = new Workout
                    {
                        Date = workout.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        ActivityType = workout.ActivityType ?? string.Empty,
                        Distance = workout.Distance,
                        Duration = workout.Duration,
                        Calories = workout.Calories,
                    };

                    // MetricsData hinzufügen
                    // if (workout.MetricsData != null)
                    // {
                    //    foreach (var metric in workout.MetricsData)
                    //    {
                    //        grpcWorkout.MetricsData.Add(metric.Key, metric.Value);
                    //    }
                    // }
                    grpcRequest.RecentWorkouts.Add(grpcWorkout);
                }
            }

            // gRPC-Call durchführen!
            var grpcResponse = await this.workoutServiceClient.GetWorkoutAnalysisAsync(grpcRequest, cancellationToken: cancellationToken);

            // gRPC-Response zu DTO konvertieren
            var response = new AIWorkoutAnalysisResponseDto
            {
                Analysis = grpcResponse.Analysis,
                KeyInsights = grpcResponse.KeyInsights.ToList(),
                Recommendations = grpcResponse.Recommendations.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                    ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,

                // AnalysisType = grpcResponse.AnalysisType
            };

            this.logger.LogInformation("gRPC: Workout analysis response received successfully from {Source}", response.Source);
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error getting workout analysis");
            throw;
        }
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetPerformanceTrendsAsync(int athleteId, string timeFrame = "month", CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation(
            "gRPC: Requesting performance trends for athlete: {AthleteId}, timeFrame: {TimeFrame}",
            athleteId, timeFrame);

        try
        {
            var grpcRequest = new PerformanceTrendsRequest
            {
                AthleteId = athleteId,
                TimeFrame = timeFrame,
            };

            var grpcResponse = await this.workoutServiceClient.GetPerformanceTrendsAsync(grpcRequest, cancellationToken: cancellationToken);

            var response = new AIWorkoutAnalysisResponseDto
            {
                Analysis = grpcResponse.Analysis,
                KeyInsights = grpcResponse.KeyInsights.ToList(),
                Recommendations = grpcResponse.Recommendations.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                    ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,

                // AnalysisType = grpcResponse.AnalysisType
            };

            this.logger.LogInformation("gRPC: Performance trends response received successfully");
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error getting performance trends");
            throw;
        }
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetTrainingRecommendationsAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("gRPC: Requesting training recommendations for athlete: {AthleteId}", athleteId);

        try
        {
            var grpcRequest = new TrainingRecommendationsRequest
            {
                AthleteId = athleteId,
            };

            var grpcResponse = await this.workoutServiceClient.GetTrainingRecommendationsAsync(grpcRequest, cancellationToken: cancellationToken);

            var response = new AIWorkoutAnalysisResponseDto
            {
                Analysis = grpcResponse.Analysis,
                KeyInsights = grpcResponse.KeyInsights.ToList(),
                Recommendations = grpcResponse.Recommendations.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                    ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,

                // AnalysisType = grpcResponse.AnalysisType
            };

            this.logger.LogInformation("gRPC: Training recommendations response received successfully");
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error getting training recommendations");
            throw;
        }
    }

    public async Task<AIWorkoutAnalysisResponseDto> AnalyzeHealthMetricsAsync(int athleteId, List<AIWorkoutDataDto> recentWorkouts, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("gRPC: Requesting health metrics analysis for athlete: {AthleteId}", athleteId);

        try
        {
            var grpcRequest = new HealthAnalysisRequest
            {
                AthleteId = athleteId,
            };

            // Workouts hinzufügen
            if (recentWorkouts != null)
            {
                foreach (var workout in recentWorkouts)
                {
                    var grpcWorkout = new Workout
                    {
                        Date = workout.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        ActivityType = workout.ActivityType ?? string.Empty,
                        Distance = workout.Distance,
                        Duration = workout.Duration,
                        Calories = workout.Calories,
                    };

                    // if (workout.MetricsData != null)
                    // {
                    //    foreach (var metric in workout.MetricsData)
                    //    {
                    //        grpcWorkout.MetricsData.Add(metric.Key, metric.Value);
                    //    }
                    // }
                    grpcRequest.RecentWorkouts.Add(grpcWorkout);
                }
            }

            var grpcResponse = await this.workoutServiceClient.AnalyzeHealthMetricsAsync(grpcRequest, cancellationToken: cancellationToken);

            var response = new AIWorkoutAnalysisResponseDto
            {
                Analysis = grpcResponse.Analysis,
                KeyInsights = grpcResponse.KeyInsights.ToList(),
                Recommendations = grpcResponse.Recommendations.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                    ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,

                // AnalysisType = grpcResponse.AnalysisType
            };

            this.logger.LogInformation("gRPC: Health metrics analysis response received successfully");
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error analyzing health metrics");
            throw;
        }
    }

    // GoogleGemini Workout Analysis
    public async Task<AIWorkoutAnalysisResponseDto> GetGoogleGeminiWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation(
            "gRPC: Requesting GoogleGemini workout analysis for {WorkoutCount} workouts",
            request.RecentWorkouts?.Count ?? 0);

        try
        {
            // Konvertiere DTO zu gRPC Request
            var grpcRequest = new WorkoutAnalysisRequest
            {
                AnalysisType = request.AnalysisType ?? "General",
                PreferredAiProvider = "googlegemini", // Zwinge GoogleGemini
            };

            // AthleteProfile hinzufügen
            if (request.AthleteProfile != null)
            {
                grpcRequest.AthleteProfile = new AthleteProfile
                {
                    Name = request.AthleteProfile.Name ?? string.Empty,
                    FitnessLevel = request.AthleteProfile.FitnessLevel ?? string.Empty,
                    PrimaryGoal = request.AthleteProfile.PrimaryGoal ?? string.Empty,
                };
            }

            // Workouts hinzufügen
            if (request.RecentWorkouts != null)
            {
                foreach (var workout in request.RecentWorkouts)
                {
                    var grpcWorkout = new Workout
                    {
                        Date = workout.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        ActivityType = workout.ActivityType ?? string.Empty,
                        Distance = workout.Distance,
                        Duration = workout.Duration,
                        Calories = workout.Calories,
                    };

                    // if (workout.MetricsData != null)
                    // {
                    //    foreach (var metric in workout.MetricsData)
                    //    {
                    //        grpcWorkout.MetricsData.Add(metric.Key, metric.Value);
                    //    }
                    // }
                    grpcRequest.RecentWorkouts.Add(grpcWorkout);
                }
            }

            // Verwende die GoogleGemini-spezifische Methode
            var grpcResponse = await this.workoutServiceClient.AnalyzeGoogleGeminiWorkoutsAsync(grpcRequest, cancellationToken: cancellationToken);

            var response = new AIWorkoutAnalysisResponseDto
            {
                Analysis = grpcResponse.Analysis,
                KeyInsights = grpcResponse.KeyInsights.ToList(),
                Recommendations = grpcResponse.Recommendations.ToList(),
                GeneratedAt = DateTime.TryParse(grpcResponse.GeneratedAt, out var parsedDate)
                    ? parsedDate : DateTime.UtcNow,
                Source = grpcResponse.Source,

                // AnalysisType = grpcResponse.AnalysisType
            };

            this.logger.LogInformation("gRPC: GoogleGemini workout analysis response received successfully");
            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error getting GoogleGemini workout analysis");
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // gRPC Health-Check durchführen
            var grpcRequest = new HealthCheckRequest();
            var grpcResponse = await this.workoutServiceClient.CheckHealthAsync(grpcRequest, cancellationToken: cancellationToken);

            this.logger.LogInformation(
                "gRPC: Health check completed - Healthy: {IsHealthy}, Message: {Message}",
                grpcResponse.IsHealthy, grpcResponse.Message);

            return grpcResponse.IsHealthy;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "gRPC: Health check failed");
            return false;
        }
    }

    public void Dispose()
    {
        this.channel?.Dispose();
        GC.SuppressFinalize(this);
    }
}