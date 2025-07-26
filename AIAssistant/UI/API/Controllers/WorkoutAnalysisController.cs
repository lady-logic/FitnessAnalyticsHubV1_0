namespace AIAssistant.UI.API.Controllers;

using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WorkoutAnalysisController : ControllerBase
{
    private readonly IWorkoutAnalysisService workoutAnalysisService;
    private readonly ILogger<WorkoutAnalysisController> logger;

    public WorkoutAnalysisController(
        IWorkoutAnalysisService workoutAnalysisService,
        ILogger<WorkoutAnalysisController> logger)
    {
        this.workoutAnalysisService = workoutAnalysisService;
        this.logger = logger;
    }

    [HttpPost("analyze/huggingface")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeHuggingFaceWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            this.logger.LogInformation(
                "Analyzing workouts with HuggingFace for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await this.workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing workouts with HuggingFace");
            return this.StatusCode(500, "An error occurred while analyzing workouts");
        }
    }

    [HttpPost("analyze/googlegemini")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeGoogleGeminiWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            this.logger.LogInformation(
                "Analyzing workouts with GoogleGemini for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await this.workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(request);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing workouts with GoogleGemini");
            return this.StatusCode(500, "An error occurred while analyzing workouts");
        }
    }

    // Performance Trends Endpoint
    [HttpGet("performance-trends/{athleteId}")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzePerformanceTrends(
        int athleteId,
        [FromQuery] string timeFrame = "month")
    {
        try
        {
            this.logger.LogInformation(
                "Analyzing performance trends for athlete: {AthleteId}, timeFrame: {TimeFrame}",
                athleteId, timeFrame);

            // Erstelle Request für Performance Trends
            var request = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Trends",
                RecentWorkouts = this.GetDemoWorkouts(athleteId, timeFrame), // Später durch echte Daten ersetzen
                AthleteProfile = this.GetDemoAthleteProfile(athleteId), // Später durch echte Daten ersetzen
                AdditionalContext = new Dictionary<string, object>
                {
                    { "timeFrame", timeFrame },
                    { "athleteId", athleteId },
                },
            };

            var result = await this.workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing performance trends for athlete {AthleteId}", athleteId);
            return this.StatusCode(500, "An error occurred while analyzing performance trends");
        }
    }

    // Training Recommendations Endpoint
    [HttpGet("recommendations/{athleteId}")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> GetTrainingRecommendations(
        int athleteId)
    {
        try
        {
            this.logger.LogInformation("Getting training recommendations for athlete: {AthleteId}", athleteId);

            var request = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Recommendations",
                RecentWorkouts = this.GetDemoWorkouts(athleteId, "week"),
                AthleteProfile = this.GetDemoAthleteProfile(athleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "training_optimization" },
                    { "athleteId", athleteId },
                },
            };

            var result = await this.workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting training recommendations for athlete {AthleteId}", athleteId);
            return this.StatusCode(500, "An error occurred while getting training recommendations");
        }
    }

    // Health Metrics Analysis Endpoint
    [HttpPost("health-analysis")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeHealthMetrics(
        [FromBody] HealthAnalysisRequestDto request)
    {
        try
        {
            this.logger.LogInformation("Analyzing health metrics for athlete: {AthleteId}", request.AthleteId);

            var analysisRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Health",
                RecentWorkouts = request.RecentWorkouts,
                AthleteProfile = this.GetDemoAthleteProfile(request.AthleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "injury_prevention" },
                    { "health_analysis", true },
                    { "athleteId", request.AthleteId },
                },
            };

            var result = await this.workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing health metrics for athlete {AthleteId}", request.AthleteId);
            return this.StatusCode(500, "An error occurred while analyzing health metrics");
        }
    }

    // Health Check für WorkoutAnalysis Service
    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            // Einfache Test-Anfrage
            var testRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Health Check",
                RecentWorkouts = new List<WorkoutDataDto>
                {
                    new WorkoutDataDto
                    {
                        Date = DateTime.Now.AddDays(-1),
                        ActivityType = "Run",
                        Distance = 5.0,
                        Duration = 1800,
                        Calories = 350,
                    },
                },
                AthleteProfile = new AthleteProfileDto
                {
                    Name = "Test User",
                    FitnessLevel = "Intermediate",
                    PrimaryGoal = "Health Check",
                },
            };

            var result = await this.workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(testRequest);

            return this.Ok(new
            {
                status = "healthy",
                message = "Workout analysis service is responding",
                timestamp = DateTime.UtcNow,
                analysisGenerated = !string.IsNullOrEmpty(result.Analysis),
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Workout analysis health check failed");
            return this.StatusCode(503, new
            {
                status = "unhealthy",
                message = ex.Message,
                timestamp = DateTime.UtcNow,
            });
        }
    }

    private List<WorkoutDataDto> GetDemoWorkouts(int athleteId, string timeFrame)
    {
        return new List<WorkoutDataDto>
        {
            new WorkoutDataDto
            {
                Date = DateTime.Now.AddDays(-1),
                ActivityType = "Run",
                Distance = 5.2,
                Duration = 1800,
                Calories = 350,
                MetricsData = new Dictionary<string, double> { { "heartRate", 145 } },
            },
            new WorkoutDataDto
            {
                Date = DateTime.Now.AddDays(-3),
                ActivityType = "Ride",
                Distance = 24.8,
                Duration = 4500,
                Calories = 890,
                MetricsData = new Dictionary<string, double> { { "heartRate", 132 } },
            },
            new WorkoutDataDto
            {
                Date = DateTime.Now.AddDays(-5),
                ActivityType = "Run",
                Distance = 3.1,
                Duration = 1080,
                Calories = 245,
                MetricsData = new Dictionary<string, double> { { "heartRate", 128 } },
            },
        };
    }

    private AthleteProfileDto GetDemoAthleteProfile(int athleteId)
    {
        return new AthleteProfileDto
        {
            Id = athleteId.ToString(),
            Name = "Demo User",
            FitnessLevel = "Intermediate",
            PrimaryGoal = "Endurance Improvement",
            Preferences = new Dictionary<string, object>
            {
                { "preferredActivities", new[] { "Run", "Ride" } },
                { "trainingDays", 4 },
            },
        };
    }
}
