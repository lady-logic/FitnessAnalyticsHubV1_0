using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.Extensions;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant._04_UI.API.Controllers;

[ApiController]
[Route("grpc-json")]
public class GrpcJsonController : ControllerBase
{
    private readonly IMotivationCoachService _motivationCoachService;
    private readonly IWorkoutAnalysisService _workoutAnalysisService;
    private readonly ILogger<GrpcJsonController> _logger;

    public GrpcJsonController(
        IMotivationCoachService motivationCoachService,
        IWorkoutAnalysisService workoutAnalysisService,
        ILogger<GrpcJsonController> logger)
    {
        _motivationCoachService = motivationCoachService;
        _workoutAnalysisService = workoutAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// gRPC-JSON Bridge für MotivationService.GetMotivation
    /// </summary>
    [HttpPost("MotivationService/GetMotivation")]
    public async Task<ActionResult> GetMotivation([FromBody] GrpcJsonMotivationRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received motivation request for athlete: {Name}",
            request.AthleteProfile?.Name ?? "Unknown");

        // Konvertiere JSON zu Application DTO (wie im REST Controller)
        var motivationRequest = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Id = Guid.NewGuid().ToString(), // Generiere eine ID
                Name = request.AthleteProfile?.Name ?? "",
                FitnessLevel = request.AthleteProfile?.FitnessLevel ?? "",
                PrimaryGoal = request.AthleteProfile?.PrimaryGoal ?? ""
            },
            IsStruggling = false, // Default value
            UpcomingWorkoutType = null, // Optional
            LastWorkout = null // Optional
        };

        // Rufe den gleichen Service auf wie der gRPC Service
        var response = await _motivationCoachService.GetHuggingFaceMotivationalMessageAsync(motivationRequest);

        // Konvertiere Response zu gRPC-JSON Format
        var grpcJsonResponse = new
        {
            motivationalMessage = response.MotivationalMessage ?? "",
            quote = response.Quote ?? "",
            actionableTips = response.ActionableTips ?? new List<string>(),
            generatedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-HuggingFace" // Hardcoded, da nicht in DTO vorhanden
        };

        _logger.LogInformation("gRPC-JSON: Successfully generated motivation response");
        return Ok(grpcJsonResponse);
    }

    /// <summary>
    /// Health Check für gRPC-JSON Bridge
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "gRPC-JSON Bridge",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            availableEndpoints = new[]
            {
                "POST /grpc-json/MotivationService/GetMotivation",
                "POST /grpc-json/WorkoutService/GetWorkoutAnalysis",
                "POST /grpc-json/WorkoutService/AnalyzeGoogleGeminiWorkouts",
                "POST /grpc-json/WorkoutService/GetPerformanceTrends",
                "POST /grpc-json/WorkoutService/GetTrainingRecommendations",
                "POST /grpc-json/WorkoutService/AnalyzeHealthMetrics",
                "GET /grpc-json/health"
            }
        });
    }

    /// <summary>
    /// gRPC-JSON Bridge für WorkoutService.GetWorkoutAnalysis
    /// </summary>
    [HttpPost("WorkoutService/GetWorkoutAnalysis")]
    public async Task<ActionResult> GetWorkoutAnalysis([FromBody] GrpcJsonWorkoutAnalysisRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received workout analysis request for {WorkoutCount} workouts",
            request.RecentWorkouts?.Length ?? 0);

        // Konvertiere JSON zu Application DTO
        var workoutAnalysisRequest = request.ToWorkoutAnalysisRequestDto();

        // Rufe den Service auf (verwende GoogleGemini als Standard)
        var response = await _workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(workoutAnalysisRequest);

        // Konvertiere Response zu gRPC-JSON Format
        var grpcJsonResponse = new
        {
            analysis = response.Analysis ?? "",
            keyInsights = response.KeyInsights ?? new List<string>(),
            recommendations = response.Recommendations ?? new List<string>(),
            generatedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-GoogleGemini"
        };

        _logger.LogInformation("gRPC-JSON: Successfully generated workout analysis response");
        return Ok(grpcJsonResponse);
    }

    /// <summary>
    /// gRPC-JSON Bridge für WorkoutService.AnalyzeGoogleGeminiWorkouts
    /// </summary>
    [HttpPost("WorkoutService/AnalyzeGoogleGeminiWorkouts")]
    public async Task<ActionResult> AnalyzeGoogleGeminiWorkouts([FromBody] GrpcJsonWorkoutAnalysisRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received GoogleGemini workout analysis request for {WorkoutCount} workouts",
            request.RecentWorkouts?.Length ?? 0);

        // Konvertiere JSON zu Application DTO
        var workoutAnalysisRequest = new WorkoutAnalysisRequestDto
        {
            AthleteProfile = request.AthleteProfile?.ToAthleteProfileDto(),
            RecentWorkouts = request.RecentWorkouts?.Select(w => w.ToWorkoutDataDto()).ToList()
                 ?? new List<WorkoutDataDto>(),
            AnalysisType = request.AnalysisType ?? "Performance"
        };

        // Verwende explizit GoogleGemini
        var response = await _workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(workoutAnalysisRequest);

        // Konvertiere Response zu gRPC-JSON Format
        var grpcJsonResponse = new
        {
            analysis = response.Analysis ?? "",
            keyInsights = response.KeyInsights ?? new List<string>(),
            recommendations = response.Recommendations ?? new List<string>(),
            generatedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-GoogleGemini"
        };

        _logger.LogInformation("gRPC-JSON: Successfully generated GoogleGemini workout analysis response");
        return Ok(grpcJsonResponse);
    }

    /// <summary>
    /// gRPC-JSON Bridge für WorkoutService.GetPerformanceTrends
    /// Placeholder - könnte erweitert werden wenn Service verfügbar
    /// </summary>
    [HttpPost("WorkoutService/GetPerformanceTrends")]
    public async Task<ActionResult> GetPerformanceTrends([FromBody] GrpcJsonPerformanceTrendsRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received performance trends request for athlete: {AthleteId}",
            request.AthleteId);

        // Da kein entsprechender Service verfügbar ist, geben wir einen Mock zurück
        var grpcJsonResponse = new
        {
            analysis = $"Performance trends analysis for athlete {request.AthleteId} over the past {request.TimeFrame} " +
                      "shows consistent training patterns and steady improvement across key metrics.",
            keyInsights = new[]
            {
                    "Consistent training frequency maintained",
                    "Progressive overload patterns observed",
                    "Recovery metrics within healthy ranges",
                    "Performance trending upward"
                },
            recommendations = new[]
            {
                    "Maintain current training consistency",
                    "Focus on progressive intensity increases",
                    "Monitor recovery indicators",
                    "Consider periodization strategies"
                },
            generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-MockPerformanceTrends"
        };

        return Ok(grpcJsonResponse);
    }

    /// <summary>
    /// gRPC-JSON Bridge für WorkoutService.GetTrainingRecommendations
    /// Placeholder - könnte erweitert werden wenn Service verfügbar
    /// </summary>
    [HttpPost("WorkoutService/GetTrainingRecommendations")]
    public async Task<ActionResult> GetTrainingRecommendations([FromBody] GrpcJsonTrainingRecommendationsRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received training recommendations request for athlete: {AthleteId}",
            request.AthleteId);

        // Da kein entsprechender Service verfügbar ist, geben wir einen Mock zurück
        var grpcJsonResponse = new
        {
            analysis = $"Training recommendations for athlete {request.AthleteId} based on current fitness profile " +
                      "and training history suggest focusing on balanced progression and recovery.",
            keyInsights = new[]
            {
                    "Current training load is appropriate",
                    "Room for intensity optimization",
                    "Recovery patterns are sustainable",
                    "Skill development opportunities identified"
                },
            recommendations = new[]
            {
                    "Incorporate 2-3 high-intensity sessions per week",
                    "Add cross-training activities for variety",
                    "Focus on technique refinement",
                    "Ensure adequate sleep and nutrition"
                },
            generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-MockTrainingRecommendations"
        };

        return Ok(grpcJsonResponse);
    }

    /// <summary>
    /// gRPC-JSON Bridge für WorkoutService.AnalyzeHealthMetrics
    /// Placeholder - könnte erweitert werden wenn Service verfügbar
    /// </summary>
    [HttpPost("WorkoutService/AnalyzeHealthMetrics")]
    public async Task<ActionResult> AnalyzeHealthMetrics([FromBody] GrpcJsonHealthMetricsRequestDto request)
    {
        _logger.LogInformation("gRPC-JSON: Received health metrics analysis request for athlete: {AthleteId} with {WorkoutCount} workouts",
            request.AthleteId, request.RecentWorkouts?.Length ?? 0);

        var workoutCount = request.RecentWorkouts?.Length ?? 0;
        var avgCalories = request.RecentWorkouts?.Any() == true ? request.RecentWorkouts.Average(w => w.Calories) : 0;

        // Da kein entsprechender Service verfügbar ist, geben wir einen Mock zurück
        var grpcJsonResponse = new
        {
            analysis = $"Health metrics analysis for athlete {request.AthleteId} based on {workoutCount} recent workouts " +
                      $"shows healthy activity levels with an average of {avgCalories:F0} calories per session.",
            keyInsights = new[]
            {
                    $"Average caloric expenditure of {avgCalories:F0} calories per workout",
                    "Activity frequency supports cardiovascular health",
                    "Workout duration patterns are sustainable",
                    "Energy expenditure aligns with fitness goals"
                },
            recommendations = new[]
            {
                    "Continue current activity levels",
                    "Monitor heart rate during workouts",
                    "Track sleep quality and recovery",
                    "Maintain consistent hydration"
                },
            generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            source = "gRPC-JSON-MockHealthMetrics"
        };

        return Ok(grpcJsonResponse);
    }
}