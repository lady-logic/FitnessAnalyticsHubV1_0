using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitnessAnalyticsHub.Infrastructure.Services;

public class AIAssistantClientService : IAIAssistantClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIAssistantClientService> _logger;
    private readonly IConfiguration _configuration;

    public AIAssistantClientService(
        HttpClient httpClient,
        ILogger<AIAssistantClientService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        // AIAssistant Base URL aus Configuration
        var aiAssistantUrl = _configuration["AIAssistant:BaseUrl"] ?? "http://localhost:5169";
        _httpClient.BaseAddress = new Uri(aiAssistantUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _logger.LogInformation("AIAssistantClientService initialized with base URL: {BaseUrl}", aiAssistantUrl);
    }

    public async Task<AIMotivationResponseDto> GetMotivationAsync(AIMotivationRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting motivation for athlete: {AthleteName}",
            request.AthleteProfile?.Name ?? "Unknown");

        // Konvertiere zu AIAssistant DTO Format
        var aiRequest = new AIAssistantMotivationRequest
        {
            AthleteProfile = new AIAssistantAthleteProfile
            {
                Name = request.AthleteProfile?.Name ?? "Champion",
                FitnessLevel = request.AthleteProfile?.FitnessLevel ?? "Intermediate",
                PrimaryGoal = request.AthleteProfile?.PrimaryGoal ?? "General Fitness"
            },
            RecentWorkouts = request.RecentWorkouts?.Select(w => new AIAssistantWorkout
            {
                Date = w.Date,
                ActivityType = w.ActivityType,
                Distance = w.Distance,
                Duration = w.Duration,
                Calories = w.Calories
            }).ToList() ?? new List<AIAssistantWorkout>(),
            PreferredTone = request.PreferredTone ?? "Encouraging",
            ContextualInfo = request.ContextualInfo ?? ""
        };

        var json = JsonSerializer.Serialize(aiRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/MotivationCoach/motivate/huggingface", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("AIAssistant motivation request failed: {StatusCode} - {Error}",
                response.StatusCode, errorContent);

            return GetFallbackMotivation(request);
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var aiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIMotivationResponseDto
        {
            MotivationalMessage = aiResponse.GetProperty("motivationalMessage").GetString() ??
                                "Keep pushing forward! You're doing great!",
            Quote = aiResponse.TryGetProperty("quote", out var quote) ? quote.GetString() : null,
            ActionableTips = aiResponse.TryGetProperty("actionableTips", out var tips) &&
                           tips.ValueKind == JsonValueKind.Array ?
                           tips.EnumerateArray().Select(t => t.GetString()).Where(s => s != null).Cast<string>().ToList() :
                           null,
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-HuggingFace"
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting workout analysis for {WorkoutCount} workouts, type: {AnalysisType}",
            request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

        // Konvertiere zu AIAssistant DTO Format
        var aiRequest = new AIAssistantWorkoutAnalysisRequest
        {
            AthleteProfile = request.AthleteProfile != null ? new AIAssistantAthleteProfile
            {
                Name = request.AthleteProfile.Name,
                FitnessLevel = request.AthleteProfile.FitnessLevel,
                PrimaryGoal = request.AthleteProfile.PrimaryGoal
            } : null,
            RecentWorkouts = request.RecentWorkouts?.Select(w => new AIAssistantWorkout
            {
                Date = w.Date,
                ActivityType = w.ActivityType,
                Distance = w.Distance,
                Duration = w.Duration,
                Calories = w.Calories
            }).ToList() ?? new List<AIAssistantWorkout>(),
            AnalysisType = request.AnalysisType ?? "Performance",
            FocusAreas = request.FocusAreas ?? new List<string>()
        };

        var json = JsonSerializer.Serialize(aiRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //var response = await _httpClient.PostAsync("/api/WorkoutAnalysis/analyze/huggingface", content, cancellationToken);
        var response = await _httpClient.PostAsync("/api/WorkoutAnalysis/analyze/googlegemini", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("AIAssistant workout analysis request failed: {StatusCode} - {Error}",
                response.StatusCode, errorContent);

            return GetFallbackAnalysis(request);
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var aiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = aiResponse.GetProperty("analysis").GetString() ??
                      "Your workouts show consistent progress. Keep up the great work!",
            KeyInsights = aiResponse.TryGetProperty("keyInsights", out var insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = aiResponse.TryGetProperty("recommendations", out var recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.UtcNow,
            Source = "AIAssistant-HuggingFace"
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Debug/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AIAssistant health check failed");
            return false;
        }
    }

    private AIMotivationResponseDto GetFallbackMotivation(AIMotivationRequestDto request)
    {
        var athleteName = request.AthleteProfile?.Name ?? "Champion";
        return new AIMotivationResponseDto
        {
            MotivationalMessage = $"Great work, {athleteName}! Your dedication to fitness is inspiring. " +
                                "Every workout brings you closer to your goals. Keep pushing forward!",
            Quote = "Success is the sum of small efforts repeated day in and day out.",
            ActionableTips = new List<string>
            {
                "Set small, achievable goals for today",
                "Focus on consistency over perfection",
                "Celebrate every workout completed"
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "Fallback"
        };
    }

    private AIWorkoutAnalysisResponseDto GetFallbackAnalysis(AIWorkoutAnalysisRequestDto request)
    {
        var workoutCount = request.RecentWorkouts?.Count ?? 0;
        var totalDistance = request.RecentWorkouts?.Sum(w => w.Distance) ?? 0;

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Based on your {workoutCount} recent workouts covering {totalDistance:F1}km, " +
                      "your training shows good consistency and steady progress toward your fitness goals.",
            KeyInsights = new List<string>
            {
                $"Completed {workoutCount} workouts with strong consistency",
                $"Total distance of {totalDistance:F1}km demonstrates good endurance",
                "Training patterns indicate balanced workout approach",
                "Performance metrics show steady improvement"
            },
            Recommendations = new List<string>
            {
                "Continue current training schedule",
                "Gradually increase intensity by 5-10%",
                "Ensure adequate recovery between sessions",
                "Consider adding workout variety"
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "Fallback"
        };
    }
}