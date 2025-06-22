using AIAssistant.Application.Interfaces;
using AIAssistant.Application.DTOs;
using AIAssistant.Extensions;
using Grpc.Core;
using Fitnessanalyticshub;

namespace AIAssistant._04_UI.API.Services;

public class WorkoutAnalysisGrpcService : WorkoutService.WorkoutServiceBase
{
    private readonly IWorkoutAnalysisService _workoutAnalysisService;
    private readonly ILogger<WorkoutAnalysisGrpcService> _logger;

    public WorkoutAnalysisGrpcService(
        IWorkoutAnalysisService workoutAnalysisService,
        ILogger<WorkoutAnalysisGrpcService> logger)
    {
        _workoutAnalysisService = workoutAnalysisService;
        _logger = logger;
    }

    public override async Task<WorkoutAnalysisResponse> GetWorkoutAnalysis(
        WorkoutAnalysisRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received workout analysis request for {WorkoutCount} workouts",
                request.RecentWorkouts?.Count ?? 0);

            // Konvertiere gRPC Request zu Application DTO
            var analysisRequest = request.ToWorkoutAnalysisRequestDto();

            // Rufe den echten HuggingFace/GoogleGemini Service auf!
            WorkoutAnalysisResponseDto response;

            // Bestimme welcher AI-Service verwendet werden soll basierend auf Request
            var aiProvider = request.PreferredAiProvider?.ToLower() ?? "huggingface";

            if (aiProvider == "googlegemini")
            {
                response = await _workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(analysisRequest);
            }
            else
            {
                response = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);
            }

            // Konvertiere zurück zu gRPC Response
            var grpcResponse = new global::Fitnessanalyticshub.WorkoutAnalysisResponse
            {
                Analysis = response.Analysis ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Source = response.Provider ?? $"{aiProvider}-AI",
            };

            // Key Insights hinzufügen
            if (response.KeyInsights != null)
            {
                grpcResponse.KeyInsights.AddRange(response.KeyInsights);
            }

            // Recommendations hinzufügen
            if (response.Recommendations != null)
            {
                grpcResponse.Recommendations.AddRange(response.Recommendations);
            }

            _logger.LogInformation("gRPC: Successfully generated workout analysis response using {Provider}", aiProvider);
            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error generating workout analysis");

            // gRPC Exception werfen
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to generate workout analysis: {ex.Message}"));
        }
    }

    // Zusätzlicher Service für Performance Trends
    public override async Task<global::Fitnessanalyticshub.WorkoutAnalysisResponse> GetPerformanceTrends(
        global::Fitnessanalyticshub.PerformanceTrendsRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received performance trends request for athlete: {AthleteId}",
                request.AthleteId);

            // Konvertiere zu WorkoutAnalysisRequest
            var analysisRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Trends",
                RecentWorkouts = GetDemoWorkouts(request.AthleteId, request.TimeFrame),
                AthleteProfile = GetDemoAthleteProfile(request.AthleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "timeFrame", request.TimeFrame },
                    { "athleteId", request.AthleteId }
                }
            };

            var response = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);

            var grpcResponse = new global::Fitnessanalyticshub.WorkoutAnalysisResponse
            {
                Analysis = response.Analysis ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Source = response.Provider ?? "HuggingFace-AI",
                AnalysisType = "PerformanceTrends"
            };

            if (response.KeyInsights != null)
                grpcResponse.KeyInsights.AddRange(response.KeyInsights);
            if (response.Recommendations != null)
                grpcResponse.Recommendations.AddRange(response.Recommendations);

            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error getting performance trends");
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to get performance trends: {ex.Message}"));
        }
    }

    // Training Recommendations Service
    public override async Task<global::Fitnessanalyticshub.WorkoutAnalysisResponse> GetTrainingRecommendations(
        global::Fitnessanalyticshub.TrainingRecommendationsRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received training recommendations request for athlete: {AthleteId}",
                request.AthleteId);

            var analysisRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Recommendations",
                RecentWorkouts = GetDemoWorkouts(request.AthleteId, "week"),
                AthleteProfile = GetDemoAthleteProfile(request.AthleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "training_optimization" },
                    { "athleteId", request.AthleteId }
                }
            };

            var response = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);

            var grpcResponse = new global::Fitnessanalyticshub.WorkoutAnalysisResponse
            {
                Analysis = response.Analysis ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Source = response.Provider ?? "HuggingFace-AI",
                AnalysisType = "TrainingRecommendations"
            };

            if (response.KeyInsights != null)
                grpcResponse.KeyInsights.AddRange(response.KeyInsights);
            if (response.Recommendations != null)
                grpcResponse.Recommendations.AddRange(response.Recommendations);

            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error getting training recommendations");
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to get training recommendations: {ex.Message}"));
        }
    }

    // Health Metrics Analysis Service
    public override async Task<global::Fitnessanalyticshub.WorkoutAnalysisResponse> AnalyzeHealthMetrics(
        global::Fitnessanalyticshub.HealthAnalysisRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received health metrics analysis request for athlete: {AthleteId}",
                request.AthleteId);

            var analysisRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Health",
                RecentWorkouts = request.RecentWorkouts
                    .Select(w => w.ToAIAssistantWorkoutData())
                    .ToList(),
                AthleteProfile = GetDemoAthleteProfile(request.AthleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "injury_prevention" },
                    { "health_analysis", true },
                    { "athleteId", request.AthleteId }
                }
            };

            var response = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);

            var grpcResponse = new global::Fitnessanalyticshub.WorkoutAnalysisResponse
            {
                Analysis = response.Analysis ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Source = response.Provider ?? "HuggingFace-AI",
                AnalysisType = "HealthMetrics"
            };

            if (response.KeyInsights != null)
                grpcResponse.KeyInsights.AddRange(response.KeyInsights);
            if (response.Recommendations != null)
                grpcResponse.Recommendations.AddRange(response.Recommendations);

            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error analyzing health metrics");
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to analyze health metrics: {ex.Message}"));
        }
    }

    // Separate GoogleGemini Analysis Service
    public override async Task<global::Fitnessanalyticshub.WorkoutAnalysisResponse> AnalyzeGoogleGeminiWorkouts(
        global::Fitnessanalyticshub.WorkoutAnalysisRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received GoogleGemini workout analysis request for {WorkoutCount} workouts",
                request.RecentWorkouts?.Count ?? 0);

            var analysisRequest = request.ToWorkoutAnalysisRequestDto();

            // Zwinge GoogleGemini Service
            var response = await _workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(analysisRequest);

            var grpcResponse = new global::Fitnessanalyticshub.WorkoutAnalysisResponse
            {
                Analysis = response.Analysis ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Source = response.Provider ?? "GoogleGemini-AI",
            };

            if (response.KeyInsights != null)
                grpcResponse.KeyInsights.AddRange(response.KeyInsights);
            if (response.Recommendations != null)
                grpcResponse.Recommendations.AddRange(response.Recommendations);

            _logger.LogInformation("gRPC: Successfully generated GoogleGemini workout analysis response");
            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error generating GoogleGemini workout analysis");
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to generate GoogleGemini workout analysis: {ex.Message}"));
        }
    }

    // Health Check für gRPC
    public override async Task<global::Fitnessanalyticshub.HealthCheckResponse> CheckHealth(
        global::Fitnessanalyticshub.HealthCheckRequest request,
        ServerCallContext context)
    {
        try
        {
            // Einfacher Test-Request
            var testRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Health Check",
                RecentWorkouts = new List<Domain.Models.WorkoutData>
                {
                    new Domain.Models.WorkoutData
                    {
                        Date = DateTime.Now.AddDays(-1),
                        ActivityType = "Run",
                        Distance = 5.0,
                        Duration = 1800,
                        Calories = 350
                    }
                },
                AthleteProfile = new Domain.Models.AthleteProfile
                {
                    Name = "Test User",
                    FitnessLevel = "Intermediate",
                    PrimaryGoal = "Health Check"
                }
            };

            var result = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(testRequest);

            return new HealthCheckResponse
            {
                IsHealthy = !string.IsNullOrEmpty(result.Analysis),
                Message = "Workout analysis service is responding",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Health check failed");
            return new HealthCheckResponse
            {
                IsHealthy = false,
                Message = $"Health check failed: {ex.Message}",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }

    // Demo-Daten Helper (später durch echte Daten ersetzen)
    private List<Domain.Models.WorkoutData> GetDemoWorkouts(int athleteId, string timeFrame)
    {
        return new List<Domain.Models.WorkoutData>
        {
            new Domain.Models.WorkoutData
            {
                Date = DateTime.Now.AddDays(-1),
                ActivityType = "Run",
                Distance = 5.2,
                Duration = 1800,
                Calories = 350,
                MetricsData = new Dictionary<string, double> { { "heartRate", 145 } }
            },
            new Domain.Models.WorkoutData
            {
                Date = DateTime.Now.AddDays(-3),
                ActivityType = "Ride",
                Distance = 24.8,
                Duration = 4500,
                Calories = 890,
                MetricsData = new Dictionary<string, double> { { "heartRate", 132 } }
            },
            new Domain.Models.WorkoutData
            {
                Date = DateTime.Now.AddDays(-5),
                ActivityType = "Run",
                Distance = 3.1,
                Duration = 1080,
                Calories = 245,
                MetricsData = new Dictionary<string, double> { { "heartRate", 128 } }
            }
        };
    }

    private Domain.Models.AthleteProfile GetDemoAthleteProfile(int athleteId)
    {
        return new Domain.Models.AthleteProfile
        {
            Id = athleteId.ToString(),
            Name = "Demo User",
            FitnessLevel = "Intermediate",
            PrimaryGoal = "Endurance Improvement",
            Preferences = new Dictionary<string, object>
            {
                { "preferredActivities", new[] { "Run", "Ride" } },
                { "trainingDays", 4 }
            }
        };
    }
}