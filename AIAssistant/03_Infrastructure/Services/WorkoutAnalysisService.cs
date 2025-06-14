using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;
using AIAssistant.Application.DTOs;

namespace AIAssistant._03_Infrastructure.Services;

public class WorkoutAnalysisService : IWorkoutAnalysisService
{
    private readonly IAIPromptService _aiPromptService;
    private readonly ILogger<WorkoutAnalysisService> _logger;

    public WorkoutAnalysisService(
        IAIPromptService aiPromptService,
        ILogger<WorkoutAnalysisService> logger)
    {
        _aiPromptService = aiPromptService;
        _logger = logger;
    }

    public async Task<WorkoutAnalysisResponseDto> AnalyzeWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts for {WorkoutCount} recent workouts, analysis type: {AnalysisType}",
                request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

            var prompt = BuildAnalysisPrompt(request);

            // Die passende AI-Methode basierend auf Analyse-Typ wählen
            string aiResponse = request.AnalysisType?.ToLower() switch
            {
                "health" => await _aiPromptService.GetHealthAnalysisAsync(prompt),
                "performance" => await _aiPromptService.GetFitnessAnalysisAsync(prompt),
                "trends" => await _aiPromptService.GetFitnessAnalysisAsync(prompt),
                _ => await _aiPromptService.GetFitnessAnalysisAsync(prompt)
            };

            var result = ParseAnalysisResponse(aiResponse, request.AnalysisType);

            _logger.LogInformation("Successfully generated workout analysis with {InsightCount} insights and {RecommendationCount} recommendations",
                result.KeyInsights?.Count ?? 0, result.Recommendations?.Count ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts");

            // Fallback analysis
            return GetFallbackAnalysis(request);
        }
    }

    // Legacy method für Backwards Compatibility
    public async Task<WorkoutAnalysisResponseDto> AnalyzeHuggingFaceWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        return await AnalyzeWorkoutsAsync(request);
    }

    private string BuildAnalysisPrompt(WorkoutAnalysisRequestDto request)
    {
        if (request.RecentWorkouts == null || !request.RecentWorkouts.Any())
        {
            return "No recent workout data available for analysis. Please provide workout data to generate insights.";
        }

        var workoutsData = string.Join("\n", request.RecentWorkouts.Select(w =>
            $"Date: {w.Date:yyyy-MM-dd}, Type: {w.ActivityType}, Distance: {w.Distance}km, " +
            $"Duration: {TimeSpan.FromSeconds(w.Duration):hh\\:mm\\:ss}, Calories: {w.Calories}"
        ));

        var athleteContext = request.AthleteProfile != null ?
            $"\nAthlete Level: {request.AthleteProfile.FitnessLevel}\nPrimary Goal: {request.AthleteProfile.PrimaryGoal}" : "";

        // Spezifischer Prompt basierend auf Analyse-Typ
        var analysisPrompt = request.AnalysisType?.ToLower() switch
        {
            "health" => BuildHealthAnalysisPrompt(workoutsData, athleteContext),
            "performance" => BuildPerformanceAnalysisPrompt(workoutsData, athleteContext),
            "trends" => BuildTrendsAnalysisPrompt(workoutsData, athleteContext),
            _ => BuildGeneralAnalysisPrompt(workoutsData, athleteContext, request.AnalysisType ?? "General")
        };

        return analysisPrompt;
    }

    private string BuildHealthAnalysisPrompt(string workoutsData, string athleteContext)
    {
        return $@"As a health and fitness expert, analyze the following workout data for health insights:

WORKOUT DATA:
{workoutsData}
{athleteContext}

Provide a health-focused analysis covering:

HEALTH ANALYSIS:
- Training load assessment (is it appropriate/excessive?)
- Recovery patterns and recommendations
- Injury prevention insights
- Cardiovascular health indicators

KEY INSIGHTS:
- 3-4 specific health-related observations
- Warning signs if any

RECOMMENDATIONS:
- Health-focused actionable advice
- Recovery strategies
- Training modifications for optimal health

Keep your response focused on health and injury prevention.";
    }

    private string BuildPerformanceAnalysisPrompt(string workoutsData, string athleteContext)
    {
        return $@"As a performance coach, analyze the following workout data for performance optimization:

WORKOUT DATA:
{workoutsData}
{athleteContext}

Provide a performance-focused analysis covering:

PERFORMANCE ANALYSIS:
- Progress evaluation and trends
- Performance strengths and weaknesses
- Training efficiency assessment
- Goal achievement potential

KEY INSIGHTS:
- 3-4 specific performance observations
- Areas of improvement identified

RECOMMENDATIONS:
- Performance optimization strategies
- Training intensity adjustments
- Specific techniques for improvement

Focus on athletic performance and competitive improvement.";
    }

    private string BuildTrendsAnalysisPrompt(string workoutsData, string athleteContext)
    {
        return $@"As a data analyst specializing in fitness trends, analyze the following workout patterns:

WORKOUT DATA:
{workoutsData}
{athleteContext}

Provide a trend-focused analysis covering:

TRENDS ANALYSIS:
- Training consistency patterns over time
- Performance progression or regression
- Weekly/monthly patterns identification
- Workout variety and distribution

KEY INSIGHTS:
- 3-4 significant trend observations
- Pattern recognition findings

RECOMMENDATIONS:
- Trend-based training advice
- Consistency improvement strategies
- Future planning suggestions

Focus on patterns, trends, and long-term progression analysis.";
    }

    private string BuildGeneralAnalysisPrompt(string workoutsData, string athleteContext, string analysisType)
    {
        return $@"As a fitness expert, provide a comprehensive analysis of the following workout data:

WORKOUT DATA:
{workoutsData}
{athleteContext}

Analysis focus: {analysisType}

Provide a detailed fitness analysis covering:

ANALYSIS:
- Overall workout assessment
- Training effectiveness evaluation
- Progress indicators
- Areas for improvement

KEY INSIGHTS:
- 3-4 specific observations from the data
- Important patterns or trends
- Performance highlights

RECOMMENDATIONS:
- Actionable training advice
- Specific improvement strategies
- Goal-oriented suggestions

Provide practical, actionable insights for fitness improvement.";
    }

    private WorkoutAnalysisResponseDto ParseAnalysisResponse(string aiResponse, string? analysisType)
    {
        var response = new WorkoutAnalysisResponseDto
        {
            Analysis = ExtractAnalysisSection(aiResponse),
            KeyInsights = ExtractKeyInsights(aiResponse),
            Recommendations = ExtractRecommendations(aiResponse),
            GeneratedAt = DateTime.UtcNow
        };

        return response;
    }

    private string ExtractAnalysisSection(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return "Unable to generate analysis at this time. Please try again later.";

        // Suche nach verschiedenen Analysis-Section Headers
        var analysisHeaders = new[] { "ANALYSIS:", "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:" };

        foreach (var header in analysisHeaders)
        {
            if (aiResponse.Contains(header, StringComparison.OrdinalIgnoreCase))
            {
                var analysisParts = aiResponse.Split(header, StringSplitOptions.RemoveEmptyEntries);
                if (analysisParts.Length > 1)
                {
                    var analysisSection = analysisParts[1].Split(new[] { "KEY INSIGHTS:", "RECOMMENDATIONS:" },
                        StringSplitOptions.RemoveEmptyEntries)[0];

                    var cleanAnalysis = analysisSection.Trim();
                    if (!string.IsNullOrWhiteSpace(cleanAnalysis) && cleanAnalysis.Length > 20)
                        return cleanAnalysis;
                }
            }
        }

        // Fallback: Nimm den ersten bedeutsamen Teil der Response
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var analysisLines = new List<string>();

        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            if (string.IsNullOrWhiteSpace(cleanLine)) continue;

            // Stoppe bei strukturierten Abschnitten
            if (cleanLine.StartsWith("KEY INSIGHTS", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("RECOMMENDATIONS", StringComparison.OrdinalIgnoreCase))
                break;

            analysisLines.Add(cleanLine);

            // Begrenze auf sinnvolle Länge
            if (analysisLines.Count >= 5) break;
        }

        var result = analysisLines.Any() ? string.Join(" ", analysisLines) :
                    "Your workout data shows consistent training patterns. Continue with your current approach while focusing on gradual progression.";

        // Begrenze die Länge
        if (result.Length > 400)
        {
            var sentences = result.Split('.', StringSplitOptions.RemoveEmptyEntries);
            result = string.Join(". ", sentences.Take(4)) + ".";
        }

        return result;
    }

    private List<string>? ExtractKeyInsights(string aiResponse)
    {
        return ExtractListSection(aiResponse, new[] { "KEY INSIGHTS:", "INSIGHTS:" });
    }

    private List<string>? ExtractRecommendations(string aiResponse)
    {
        return ExtractListSection(aiResponse, new[] { "RECOMMENDATIONS:", "ADVICE:" });
    }

    private List<string>? ExtractListSection(string aiResponse, string[] sectionHeaders)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return null;

        var items = new List<string>();

        foreach (var header in sectionHeaders)
        {
            if (aiResponse.Contains(header, StringComparison.OrdinalIgnoreCase))
            {
                var headerIndex = aiResponse.IndexOf(header, StringComparison.OrdinalIgnoreCase);
                var section = aiResponse.Substring(headerIndex + header.Length);

                // Stoppe bei nächstem Header
                var nextHeaders = new[] { "ANALYSIS:", "KEY INSIGHTS:", "RECOMMENDATIONS:", "ADVICE:", "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:" };
                foreach (var nextHeader in nextHeaders)
                {
                    if (nextHeader != header && section.Contains(nextHeader, StringComparison.OrdinalIgnoreCase))
                    {
                        var nextHeaderIndex = section.IndexOf(nextHeader, StringComparison.OrdinalIgnoreCase);
                        section = section.Substring(0, nextHeaderIndex);
                        break;
                    }
                }

                var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var cleanLine = line.Trim()
                        .TrimStart('-', '*', '•', '1', '2', '3', '4', '5', '.', ' ')
                        .Trim();

                    if (!string.IsNullOrWhiteSpace(cleanLine) &&
                        cleanLine.Length > 15 &&
                        cleanLine.Length < 200)
                    {
                        items.Add(cleanLine);
                        if (items.Count >= 5) break; // Maximal 5 Items
                    }
                }
                break;
            }
        }

        return items.Any() ? items : null;
    }

    private WorkoutAnalysisResponseDto GetFallbackAnalysis(WorkoutAnalysisRequestDto request)
    {
        var workoutCount = request.RecentWorkouts?.Count ?? 0;
        var totalDistance = request.RecentWorkouts?.Sum(w => w.Distance) ?? 0;
        var totalDuration = request.RecentWorkouts?.Sum(w => w.Duration) ?? 0;
        var avgCalories = request.RecentWorkouts?.Any() == true ?
            request.RecentWorkouts.Average(w => w.Calories) : 0;

        var analysisType = request.AnalysisType ?? "Performance";

        var analysis = analysisType.ToLower() switch
        {
            "health" => $"Based on your {workoutCount} recent workouts, your training load appears well-balanced. " +
                       $"Total distance of {totalDistance:F1}km over {TimeSpan.FromSeconds(totalDuration):h\\:mm} shows good cardiovascular engagement. " +
                       $"No concerning overtraining patterns detected. Your average calorie burn of {avgCalories:F0} per session indicates appropriate workout intensity.",

            "performance" => $"Your performance data shows {workoutCount} completed workouts with {totalDistance:F1}km total distance. " +
                           $"Training consistency appears strong with varied workout types. " +
                           $"Average session duration of {TimeSpan.FromSeconds(workoutCount > 0 ? totalDuration / workoutCount : 0):h\\:mm} suggests good endurance building. " +
                           $"Performance metrics indicate steady progression toward your goals.",

            "trends" => $"Training trend analysis reveals {workoutCount} workouts over the recent period. " +
                       $"Total distance progression to {totalDistance:F1}km shows positive training consistency. " +
                       $"Workout frequency and duration patterns indicate sustainable training habits. " +
                       $"Calorie expenditure trends suggest effective energy management.",

            _ => $"Comprehensive analysis of your {workoutCount} recent workouts covering {totalDistance:F1}km shows excellent training consistency. " +
                $"Your {analysisType.ToLower()} metrics indicate steady progress toward your fitness goals. " +
                $"Training load and recovery balance appears appropriate for continued improvement."
        };

        var insights = analysisType.ToLower() switch
        {
            "health" => new List<string>
            {
                $"Completed {workoutCount} workouts with no overtraining indicators",
                $"Average calorie burn of {avgCalories:F0} suggests appropriate intensity",
                "Training frequency supports good cardiovascular health",
                "No concerning health patterns detected in workout data"
            },
            "performance" => new List<string>
            {
                $"Achieved {totalDistance:F1}km total distance across {workoutCount} sessions",
                "Training consistency shows strong commitment to performance goals",
                $"Average workout intensity of {avgCalories:F0} calories is performance-oriented",
                "Workout variety supports well-rounded athletic development"
            },
            "trends" => new List<string>
            {
                $"Training frequency of {workoutCount} workouts shows consistent habit formation",
                "Distance and duration trends indicate progressive overload application",
                "Calorie expenditure patterns suggest effective workout intensity management",
                "Overall trajectory points toward continued fitness improvement"
            },
            _ => new List<string>
            {
                $"Completed {workoutCount} workouts with total distance of {totalDistance:F1}km",
                "Training consistency demonstrates commitment to fitness goals",
                "Performance metrics indicate steady improvement trajectory",
                "Workout intensity and frequency appear well-balanced"
            }
        };

        var recommendations = analysisType.ToLower() switch
        {
            "health" => new List<string>
            {
                "Continue current training schedule to maintain health benefits",
                "Monitor recovery signs and adjust intensity if feeling fatigued",
                "Ensure adequate sleep and nutrition to support training load",
                "Consider adding mobility work to prevent injury"
            },
            "performance" => new List<string>
            {
                "Gradually increase workout intensity by 5-10% for performance gains",
                "Add interval training to boost speed and power development",
                "Consider performance testing to track specific improvements",
                "Incorporate sport-specific drills for targeted skill development"
            },
            "trends" => new List<string>
            {
                "Maintain current training frequency for continued positive trends",
                "Plan progressive increases in distance and duration",
                "Track weekly trends to identify optimal training patterns",
                "Set monthly goals based on current progression rate"
            },
            _ => new List<string>
            {
                "Continue with current training schedule and intensity",
                "Gradually increase workout difficulty by 5-10% every 2-3 weeks",
                "Ensure adequate rest between intense training sessions",
                "Consider adding variety to workout types for balanced development"
            }
        };

        return new WorkoutAnalysisResponseDto
        {
            Analysis = analysis,
            KeyInsights = insights,
            Recommendations = recommendations,
            GeneratedAt = DateTime.UtcNow
        };
    }
}