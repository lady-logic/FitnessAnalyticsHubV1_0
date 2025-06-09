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

    public async Task<WorkoutAnalysisResponseDto> AnalyzeOpenAIWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        try
        {
            // Prompt für OpenAI erstellen
            var prompt = BuildAnalysisPrompt(request);

            // OpenAI API aufrufen
            var aiResponse = await _aiPromptService.GetOpenAICompletionAsync(prompt);

            // Antwort parsen und strukturieren
            return ParseAnalysisResponse(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts");
            throw;
        }
    }

    public async Task<WorkoutAnalysisResponseDto> AnalyzeClaudeWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        try
        {
            // Prompt für OpenAI erstellen
            var prompt = BuildAnalysisPrompt(request);

            // OpenAI API aufrufen
            var aiResponse = await _aiPromptService.GetClaudeCompletionAsync(prompt);

            // Antwort parsen und strukturieren
            return ParseAnalysisResponse(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts");
            throw;
        }
    }

    private string BuildAnalysisPrompt(Application.DTOs.WorkoutAnalysisRequestDto request)
    {
        // Formatiere die Workout-Daten für den Prompt
        var workoutsData = string.Join("\n", request.RecentWorkouts.Select(w =>
            $"Date: {w.Date:yyyy-MM-dd}, Type: {w.ActivityType}, Distance: {w.Distance}km, " +
            $"Duration: {TimeSpan.FromSeconds(w.Duration):hh\\:mm\\:ss}, Calories: {w.Calories}"
        ));

        // Athlet-Kontext hinzufügen, falls vorhanden
        var athleteContext = request.AthleteProfile != null ?
            $"\nAthlete Level: {request.AthleteProfile.FitnessLevel}\nPrimary Goal: {request.AthleteProfile.PrimaryGoal}" : "";

        return $@"As a fitness expert, analyze the following workout data:

{workoutsData}
{athleteContext}

Analysis type requested: {request.AnalysisType}

Please provide:
1. A detailed analysis of the workout patterns and performance
2. 3-5 key insights from the data
3. Specific recommendations for improvement

Format your response in clear sections for Analysis, Key Insights, and Recommendations.";
    }

    private WorkoutAnalysisResponseDto ParseAnalysisResponse(string aiResponse)
    {
        // Parsen der strukturierten Antwort
        var response = new WorkoutAnalysisResponseDto
        {
            Analysis = aiResponse,
            GeneratedAt = DateTime.UtcNow
        };

        // Einfache Extraktion von Insights und Empfehlungen
        if (aiResponse.Contains("Key Insights:"))
        {
            var insightsSection = aiResponse.Split("Key Insights:")[1].Split("Recommendations:")[0];
            response.KeyInsights = insightsSection
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith("-", StringComparison.OrdinalIgnoreCase))
                .Select(line => line.TrimStart('-', ' '))
                .ToList();
        }

        if (aiResponse.Contains("Recommendations:"))
        {
            var recommendationsSection = aiResponse.Split("Recommendations:")[1];
            response.Recommendations = recommendationsSection
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith("-", StringComparison.OrdinalIgnoreCase))
                .Select(line => line.TrimStart('-', ' '))
                .ToList();
        }

        return response;
    }

}
