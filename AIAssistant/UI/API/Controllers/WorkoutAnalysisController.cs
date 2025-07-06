using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant._04_UI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutAnalysisController : ControllerBase
{
    private readonly IWorkoutAnalysisService _workoutAnalysisService;
    private readonly ILogger<WorkoutAnalysisController> _logger;

    public WorkoutAnalysisController(
        IWorkoutAnalysisService workoutAnalysisService,
        ILogger<WorkoutAnalysisController> logger)
    {
        _workoutAnalysisService = workoutAnalysisService;
        _logger = logger;
    }

    [HttpPost("analyze/huggingface")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeHuggingFaceWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts with HuggingFace for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts with HuggingFace");
            return StatusCode(500, "An error occurred while analyzing workouts");
        }
    }

    [HttpPost("analyze/googlegemini")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeGoogleGeminiWorkouts(
        [FromBody] WorkoutAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts with GoogleGemini for analysis type: {AnalysisType}",
                request.AnalysisType);

            var result = await _workoutAnalysisService.AnalyzeGoogleGeminiWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts with GoogleGemini");
            return StatusCode(500, "An error occurred while analyzing workouts");
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
            _logger.LogInformation("Analyzing performance trends for athlete: {AthleteId}, timeFrame: {TimeFrame}",
                athleteId, timeFrame);

            // Erstelle Request für Performance Trends
            var request = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Trends",
                RecentWorkouts = GetDemoWorkouts(athleteId, timeFrame), // Später durch echte Daten ersetzen
                AthleteProfile = GetDemoAthleteProfile(athleteId), // Später durch echte Daten ersetzen
                AdditionalContext = new Dictionary<string, object>
                {
                    { "timeFrame", timeFrame },
                    { "athleteId", athleteId }
                }
            };

            var result = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance trends for athlete {AthleteId}", athleteId);
            return StatusCode(500, "An error occurred while analyzing performance trends");
        }
    }

    // Training Recommendations Endpoint 
    [HttpGet("recommendations/{athleteId}")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> GetTrainingRecommendations(
        int athleteId)
    {
        try
        {
            _logger.LogInformation("Getting training recommendations for athlete: {AthleteId}", athleteId);

            var request = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Recommendations",
                RecentWorkouts = GetDemoWorkouts(athleteId, "week"),
                AthleteProfile = GetDemoAthleteProfile(athleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "training_optimization" },
                    { "athleteId", athleteId }
                }
            };

            var result = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting training recommendations for athlete {AthleteId}", athleteId);
            return StatusCode(500, "An error occurred while getting training recommendations");
        }
    }

    // Health Metrics Analysis Endpoint
    [HttpPost("health-analysis")]
    public async Task<ActionResult<WorkoutAnalysisResponseDto>> AnalyzeHealthMetrics(
        [FromBody] HealthAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing health metrics for athlete: {AthleteId}", request.AthleteId);

            var analysisRequest = new WorkoutAnalysisRequestDto
            {
                AnalysisType = "Health",
                RecentWorkouts = request.RecentWorkouts,
                AthleteProfile = GetDemoAthleteProfile(request.AthleteId),
                AdditionalContext = new Dictionary<string, object>
                {
                    { "focus", "injury_prevention" },
                    { "health_analysis", true },
                    { "athleteId", request.AthleteId }
                }
            };

            var result = await _workoutAnalysisService.AnalyzeHuggingFaceWorkoutsAsync(analysisRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing health metrics for athlete {AthleteId}", request.AthleteId);
            return StatusCode(500, "An error occurred while analyzing health metrics");
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

            return Ok(new
            {
                status = "healthy",
                message = "Workout analysis service is responding",
                timestamp = DateTime.UtcNow,
                analysisGenerated = !string.IsNullOrEmpty(result.Analysis)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workout analysis health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    // Demo-Daten Helper (später durch echte Daten von deiner WebAPI ersetzen)
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
