namespace FitnessAnalyticsHub.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GrpcJsonClientService : IAIAssistantClientService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<GrpcJsonClientService> logger;
    private readonly IConfiguration configuration;

    public GrpcJsonClientService(
        HttpClient httpClient,
        ILogger<GrpcJsonClientService> logger,
        IConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.configuration = configuration;

        // AIAssistant Base URL für gRPC-JSON Endpunkte
        string aiAssistantUrl = this.configuration["AIAssistant:BaseUrl"] ?? "http://localhost:5169";
        this.httpClient.BaseAddress = new Uri(aiAssistantUrl);
        this.httpClient.Timeout = TimeSpan.FromSeconds(30);

        this.logger.LogInformation("GrpcJsonClientService initialized with base URL: {BaseUrl}", aiAssistantUrl);
    }

    public async Task<AIMotivationResponseDto> GetMotivationAsync(AIMotivationRequestDto request, CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "gRPC-JSON: Requesting motivation for athlete: {AthleteName}",
            request.AthleteProfile?.Name ?? "Unknown");

        // Erstelle JSON im gRPC-Format (nicht REST-Format!)
        var grpcJsonRequest = new
        {
            athleteProfile = new
            {
                name = request.AthleteProfile?.Name ?? string.Empty,
                fitnessLevel = request.AthleteProfile?.FitnessLevel ?? string.Empty,
                primaryGoal = request.AthleteProfile?.PrimaryGoal ?? string.Empty,
            },
            isStruggling = false,
            upcomingWorkoutType = (string?)null,
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        // HTTP POST zu gRPC-JSON Endpunkt
        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/MotivationService/GetMotivation", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON motivation request failed: {StatusCode} - {Error}",
                response.StatusCode, errorContent);

            return this.GetFallbackMotivation(request);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Konvertiere gRPC-JSON Response zurück zu DTO
        return new AIMotivationResponseDto
        {
            MotivationalMessage = grpcJsonResponse.TryGetProperty("motivationalMessage", out JsonElement msgProp)
                     ? msgProp.GetString() ?? "Keep pushing forward! You're doing great!"
                     : "Keep pushing forward! You're doing great!",
            Quote = grpcJsonResponse.TryGetProperty("quote", out JsonElement quote) ? quote.GetString() : null,
            ActionableTips = grpcJsonResponse.TryGetProperty("actionableTips", out JsonElement tips) &&
                           tips.ValueKind == JsonValueKind.Array ?
                           tips.EnumerateArray().Select(t => t.GetString()).Where(s => s != null).Cast<string>().ToList() :
                           null,
            GeneratedAt = grpcJsonResponse.TryGetProperty("generatedAt", out JsonElement dateProp) &&
              DateTime.TryParse(dateProp.GetString(), out DateTime parsedDate)
              ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON",
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "gRPC-JSON: Requesting workout analysis for {WorkoutCount} workouts, type: {AnalysisType}",
            request.RecentWorkouts?.Count ?? 0,
            request.AnalysisType ?? "General");

        // Erstelle JSON im gRPC-Format
        var grpcJsonRequest = new
        {
            athleteProfile = request.AthleteProfile != null ? new
            {
                name = request.AthleteProfile.Name ?? string.Empty,
                fitnessLevel = request.AthleteProfile.FitnessLevel ?? string.Empty,
                primaryGoal = request.AthleteProfile.PrimaryGoal ?? string.Empty,
            }
            : null,
            recentWorkouts = request.RecentWorkouts?.Select(w => new
            {
                date = w.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                activityType = w.ActivityType,
                distance = w.Distance,
                duration = w.Duration,
                calories = w.Calories,
            }).ToArray() ?? Array.Empty<object>(),
            analysisType = request.AnalysisType ?? "Performance",
            focusAreas = request.FocusAreas ?? new List<string>(),
            preferredAiProvider = "googlegemini",
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/WorkoutService/GetWorkoutAnalysis", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON workout analysis request failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);

            return this.GetFallbackAnalysis(request);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = grpcJsonResponse.GetProperty("analysis").GetString() ??
                      "Your workouts show consistent progress. Keep up the great work!",
            KeyInsights = grpcJsonResponse.TryGetProperty("keyInsights", out JsonElement insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = grpcJsonResponse.TryGetProperty("recommendations", out JsonElement recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.TryParse(grpcJsonResponse.GetProperty("generatedAt").GetString(), out DateTime parsedDate)
                        ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON-GoogleGemini",
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetGoogleGeminiWorkoutAnalysisAsync(AIWorkoutAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "gRPC-JSON: Requesting GoogleGemini workout analysis for {WorkoutCount} workouts",
            request.RecentWorkouts?.Count ?? 0);

        // Erstelle JSON im gRPC-Format (gleich wie GetWorkoutAnalysisAsync, aber explizit GoogleGemini)
        var grpcJsonRequest = new
        {
            athleteProfile = request.AthleteProfile != null ? new
            {
                name = request.AthleteProfile.Name ?? string.Empty,
                fitnessLevel = request.AthleteProfile.FitnessLevel ?? string.Empty,
                primaryGoal = request.AthleteProfile.PrimaryGoal ?? string.Empty,
            }
            : null,
            recentWorkouts = request.RecentWorkouts?.Select(w => new
            {
                date = w.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                activityType = w.ActivityType,
                distance = w.Distance,
                duration = w.Duration,
                calories = w.Calories,
            }).ToArray() ?? Array.Empty<object>(),
            analysisType = request.AnalysisType ?? "Performance",
            focusAreas = request.FocusAreas ?? new List<string>(),
            preferredAiProvider = "googlegemini",
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/WorkoutService/AnalyzeGoogleGeminiWorkouts", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON GoogleGemini workout analysis request failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);

            return this.GetFallbackAnalysis(request);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = grpcJsonResponse.GetProperty("analysis").GetString() ??
                      "Your workouts show consistent progress. Keep up the great work!",
            KeyInsights = grpcJsonResponse.TryGetProperty("keyInsights", out JsonElement insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = grpcJsonResponse.TryGetProperty("recommendations", out JsonElement recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.TryParse(grpcJsonResponse.GetProperty("generatedAt").GetString(), out DateTime parsedDate)
                        ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON-GoogleGemini",
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetPerformanceTrendsAsync(int athleteId, CancellationToken cancellationToken, string timeFrame = "month")
    {
        this.logger.LogInformation(
            "gRPC-JSON: Requesting performance trends for athlete: {AthleteId}, timeFrame: {TimeFrame}",
            athleteId,
            timeFrame);

        var grpcJsonRequest = new
        {
            athleteId = athleteId,
            timeFrame = timeFrame,
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/WorkoutService/GetPerformanceTrends", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON performance trends request failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);

            return this.GetFallbackPerformanceTrends(athleteId, timeFrame);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = grpcJsonResponse.GetProperty("analysis").GetString() ??
                      "Performance trends analysis completed successfully.",
            KeyInsights = grpcJsonResponse.TryGetProperty("keyInsights", out JsonElement insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = grpcJsonResponse.TryGetProperty("recommendations", out JsonElement recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.TryParse(grpcJsonResponse.GetProperty("generatedAt").GetString(), out DateTime parsedDate)
                        ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON-PerformanceTrends",
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> GetTrainingRecommendationsAsync(int athleteId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("gRPC-JSON: Requesting training recommendations for athlete: {AthleteId}", athleteId);

        var grpcJsonRequest = new
        {
            athleteId = athleteId,
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/WorkoutService/GetTrainingRecommendations", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON training recommendations request failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);

            return this.GetFallbackTrainingRecommendations(athleteId);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = grpcJsonResponse.GetProperty("analysis").GetString() ??
                      "Training recommendations generated successfully.",
            KeyInsights = grpcJsonResponse.TryGetProperty("keyInsights", out JsonElement insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = grpcJsonResponse.TryGetProperty("recommendations", out JsonElement recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.TryParse(grpcJsonResponse.GetProperty("generatedAt").GetString(), out DateTime parsedDate)
                        ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON-TrainingRecommendations",
        };
    }

    public async Task<AIWorkoutAnalysisResponseDto> AnalyzeHealthMetricsAsync(int athleteId, List<AIWorkoutDataDto> recentWorkouts, CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "gRPC-JSON: Requesting health metrics analysis for athlete: {AthleteId} with {WorkoutCount} workouts",
            athleteId,
            recentWorkouts?.Count ?? 0);

        var grpcJsonRequest = new
        {
            athleteId = athleteId,
            recentWorkouts = recentWorkouts?.Select(w => new
            {
                date = w.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                activityType = w.ActivityType,
                distance = w.Distance,
                duration = w.Duration,
                calories = w.Calories,
            }).ToArray() ?? Array.Empty<object>(),
        };

        string json = JsonSerializer.Serialize(grpcJsonRequest);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync("/grpc-json/WorkoutService/AnalyzeHealthMetrics", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "gRPC-JSON health metrics analysis request failed: {StatusCode} - {Error}",
                response.StatusCode,
                errorContent);

            return this.GetFallbackHealthMetrics(athleteId, recentWorkouts);
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement grpcJsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = grpcJsonResponse.GetProperty("analysis").GetString() ??
                      "Health metrics analysis completed successfully.",
            KeyInsights = grpcJsonResponse.TryGetProperty("keyInsights", out JsonElement insights) &&
                        insights.ValueKind == JsonValueKind.Array ?
                        insights.EnumerateArray().Select(i => i.GetString()).Where(s => s != null).Cast<string>().ToList() :
                        new List<string>(),
            Recommendations = grpcJsonResponse.TryGetProperty("recommendations", out JsonElement recs) &&
                            recs.ValueKind == JsonValueKind.Array ?
                            recs.EnumerateArray().Select(r => r.GetString()).Where(s => s != null).Cast<string>().ToList() :
                            new List<string>(),
            GeneratedAt = DateTime.TryParse(grpcJsonResponse.GetProperty("generatedAt").GetString(), out DateTime parsedDate)
                        ? parsedDate : DateTime.UtcNow,
            Source = "gRPC-JSON-HealthMetrics",
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("gRPC-JSON: Checking health status");

        HttpResponseMessage response = await this.httpClient.GetAsync("/grpc-json/health", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            this.logger.LogWarning("gRPC-JSON: Health check failed with status: {StatusCode}", response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement healthResponse = JsonSerializer.Deserialize<JsonElement>(content);

        string? status = healthResponse.GetProperty("status").GetString();
        bool isHealthy = status?.Equals("healthy", StringComparison.OrdinalIgnoreCase) == true;

        this.logger.LogInformation("gRPC-JSON: Health check result: {IsHealthy}", isHealthy);
        return isHealthy;
    }

    #region Fallback Methods

    private AIMotivationResponseDto GetFallbackMotivation(AIMotivationRequestDto request)
    {
        string athleteName = request.AthleteProfile?.Name ?? "Champion";
        return new AIMotivationResponseDto
        {
            MotivationalMessage = $"Great work, {athleteName}! Your dedication to fitness is inspiring. " +
                                "Every workout brings you closer to your goals. Keep pushing forward!",
            Quote = "Success is the sum of small efforts repeated day in and day out.",
            ActionableTips = new List<string>
            {
                "Set small, achievable goals for today",
                "Focus on consistency over perfection",
                "Celebrate every workout completed",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "gRPC-JSON-Fallback",
        };
    }

    private AIWorkoutAnalysisResponseDto GetFallbackAnalysis(AIWorkoutAnalysisRequestDto request)
    {
        int workoutCount = request.RecentWorkouts?.Count ?? 0;
        double totalDistance = request.RecentWorkouts?.Sum(w => w.Distance) ?? 0;

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Based on your {workoutCount} recent workouts covering {totalDistance:F1}km, " +
                      "your training shows good consistency and steady progress toward your fitness goals.",
            KeyInsights = new List<string>
            {
                $"Completed {workoutCount} workouts with strong consistency",
                $"Total distance of {totalDistance:F1}km demonstrates good endurance",
                "Training patterns indicate balanced workout approach",
                "Performance metrics show steady improvement",
            },
            Recommendations = new List<string>
            {
                "Continue current training schedule",
                "Gradually increase intensity by 5-10%",
                "Ensure adequate recovery between sessions",
                "Consider adding workout variety",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "gRPC-JSON-Fallback",
        };
    }

    private AIWorkoutAnalysisResponseDto GetFallbackPerformanceTrends(int athleteId, string timeFrame)
    {
        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Performance trends analysis for athlete {athleteId} over the past {timeFrame} " +
                      "shows consistent training patterns and steady improvement across key metrics.",
            KeyInsights = new List<string>
            {
                "Consistent training frequency maintained",
                "Progressive overload patterns observed",
                "Recovery metrics within healthy ranges",
                "Performance trending upward",
            },
            Recommendations = new List<string>
            {
                "Maintain current training consistency",
                "Focus on progressive intensity increases",
                "Monitor recovery indicators",
                "Consider periodization strategies",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "gRPC-JSON-Fallback-PerformanceTrends",
        };
    }

    private AIWorkoutAnalysisResponseDto GetFallbackTrainingRecommendations(int athleteId)
    {
        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Training recommendations for athlete {athleteId} based on current fitness profile " +
                      "and training history suggest focusing on balanced progression and recovery.",
            KeyInsights = new List<string>
            {
                "Current training load is appropriate",
                "Room for intensity optimization",
                "Recovery patterns are sustainable",
                "Skill development opportunities identified",
            },
            Recommendations = new List<string>
            {
                "Incorporate 2-3 high-intensity sessions per week",
                "Add cross-training activities for variety",
                "Focus on technique refinement",
                "Ensure adequate sleep and nutrition",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "gRPC-JSON-Fallback-TrainingRecommendations",
        };
    }

    private AIWorkoutAnalysisResponseDto GetFallbackHealthMetrics(int athleteId, List<AIWorkoutDataDto>? recentWorkouts)
    {
        int workoutCount = recentWorkouts?.Count ?? 0;
        double avgCalories = recentWorkouts?.Any() == true ? recentWorkouts.Average(w => w.Calories) : 0;

        return new AIWorkoutAnalysisResponseDto
        {
            Analysis = $"Health metrics analysis for athlete {athleteId} based on {workoutCount} recent workouts " +
                      $"shows healthy activity levels with an average of {avgCalories:F0} calories per session.",
            KeyInsights = new List<string>
            {
                $"Average caloric expenditure of {avgCalories:F0} calories per workout",
                "Activity frequency supports cardiovascular health",
                "Workout duration patterns are sustainable",
                "Energy expenditure aligns with fitness goals",
            },
            Recommendations = new List<string>
            {
                "Continue current activity levels",
                "Monitor heart rate during workouts",
                "Track sleep quality and recovery",
                "Maintain consistent hydration",
            },
            GeneratedAt = DateTime.UtcNow,
            Source = "gRPC-JSON-Fallback-HealthMetrics",
        };
    }

    #endregion
}