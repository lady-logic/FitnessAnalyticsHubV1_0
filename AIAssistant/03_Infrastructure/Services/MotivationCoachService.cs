using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;

namespace AIAssistant._03_Infrastructure.Services;

public class MotivationCoachService : IMotivationCoachService
{
    private readonly IAIPromptService _aiPromptService;
    private readonly ILogger<MotivationCoachService> _logger;

    public MotivationCoachService(
        IAIPromptService aiPromptService,
        ILogger<MotivationCoachService> logger)
    {
        _aiPromptService = aiPromptService;
        _logger = logger;
    }

    public async Task<MotivationResponseDto> GenerateMotivationAsync(MotivationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Generating motivational message for athlete: {Name}",
                request.AthleteProfile?.Name ?? "Unknown");

            var prompt = BuildMotivationPrompt(request);

            var aiResponse = await _aiPromptService.GetMotivationAsync(prompt);

            var result = ParseMotivationResponse(aiResponse);

            _logger.LogInformation("Successfully generated motivational message with {MessageLength} characters",
                result.MotivationalMessage?.Length ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating motivational message");

            // Fallback motivational message
            return new MotivationResponseDto
            {
                MotivationalMessage = GetFallbackMotivation(request),
                Quote = "\"Success is the sum of small efforts repeated day in and day out.\" - Robert Collier",
                ActionableTips = new List<string>
                {
                    "Set small, achievable goals for today",
                    "Focus on consistency over perfection",
                    "Celebrate every small victory"
                },
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    // Legacy method für Backwards Compatibility
    public async Task<MotivationResponseDto> GetHuggingFaceMotivationalMessageAsync(
        MotivationRequestDto request)
    {
        return await GenerateMotivationAsync(request);
    }

    private string BuildMotivationPrompt(MotivationRequestDto request)
    {
        var athleteName = request.AthleteProfile?.Name ?? "Champion";
        var fitnessLevel = request.AthleteProfile?.FitnessLevel ?? "Beginner";
        var primaryGoal = request.AthleteProfile?.PrimaryGoal ?? "General Fitness";

        var lastWorkoutInfo = request.LastWorkout != null ?
            $"Last workout: {request.LastWorkout.ActivityType}, {request.LastWorkout.Distance}km, " +
            $"{TimeSpan.FromSeconds(request.LastWorkout.Duration):hh\\:mm\\:ss}" :
            "No recent workout data available";

        var motivationLevel = (request.IsStruggling) ?
            "The athlete is currently struggling with motivation and needs extra encouragement." :
            "The athlete is looking for additional motivation to stay on track.";

        // Optimiert für moderne AI Models (HuggingFace, OpenAI, etc.)
        return $@"Create a motivational fitness message for {athleteName}.

Athlete Profile:
- Fitness Level: {fitnessLevel}
- Primary Goal: {primaryGoal}
- {lastWorkoutInfo}
- {motivationLevel}

Generate a motivational response with:
1. Personal encouragement (2-3 sentences)
2. An inspiring fitness quote
3. 2-3 actionable tips

Keep it positive, personal, and energizing!

Response:";
    }

    private MotivationResponseDto ParseMotivationResponse(string aiResponse)
    {
        var response = new MotivationResponseDto
        {
            MotivationalMessage = ExtractMotivationalMessage(aiResponse),
            Quote = ExtractQuote(aiResponse),
            ActionableTips = ExtractTips(aiResponse),
            GeneratedAt = DateTime.UtcNow
        };

        return response;
    }

    private string ExtractMotivationalMessage(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return "You're doing great! Keep up the excellent work with your fitness journey.";

        // Extrahiere Hauptnachricht (ersten Absatz oder bis zum Quote/Tips)
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var messageLines = new List<string>();

        foreach (var line in lines)
        {
            var cleanLine = line.Trim();

            // Stoppe bei strukturierten Abschnitten
            if (cleanLine.StartsWith("Quote:", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("Tips:", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("Actionable", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("1.", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("2.", StringComparison.OrdinalIgnoreCase))
                break;

            // Ignoriere reine Quotes in Anführungszeichen am Anfang von Zeilen
            if (cleanLine.StartsWith('"') && cleanLine.EndsWith('"') && cleanLine.Length > 20)
                continue;

            if (!string.IsNullOrWhiteSpace(cleanLine))
                messageLines.Add(cleanLine);
        }

        var result = messageLines.Any() ? string.Join(" ", messageLines) :
                    "You're doing great! Keep up the excellent work with your fitness journey.";

        // Begrenze die Länge
        if (result.Length > 300)
        {
            var sentences = result.Split('.', StringSplitOptions.RemoveEmptyEntries);
            result = string.Join(". ", sentences.Take(3)) + ".";
        }

        return result;
    }

    private string? ExtractQuote(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return null;

        // Suche nach Zitaten in Anführungszeichen
        var quoteMatches = System.Text.RegularExpressions.Regex.Matches(aiResponse, @"""([^""]{10,})""");

        foreach (System.Text.RegularExpressions.Match match in quoteMatches)
        {
            var quote = match.Groups[1].Value.Trim();
            // Filtere motivierende Quotes (keine technischen Texte)
            if (quote.Length >= 15 && quote.Length <= 150 &&
                (quote.Contains("success") || quote.Contains("achieve") || quote.Contains("goal") ||
                 quote.Contains("dream") || quote.Contains("believe") || quote.Contains("strong") ||
                 quote.Contains("push") || quote.Contains("better")))
            {
                return quote;
            }
        }

        // Suche nach "Quote:" Label
        if (aiResponse.Contains("Quote:", StringComparison.OrdinalIgnoreCase))
        {
            var quoteParts = aiResponse.Split("Quote:", StringSplitOptions.RemoveEmptyEntries);
            if (quoteParts.Length > 1)
            {
                var quoteLine = quoteParts[1].Split('\n')[0].Trim().Trim('"', '-', '*').Trim();
                if (!string.IsNullOrWhiteSpace(quoteLine) && quoteLine.Length >= 15)
                    return quoteLine;
            }
        }

        return null;
    }

    private List<string>? ExtractTips(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return null;

        var tips = new List<string>();

        // Suche nach "Tips:" oder ähnlichen Labels
        var tipsSection = "";
        var lowerResponse = aiResponse.ToLower();

        if (lowerResponse.Contains("tips:"))
        {
            tipsSection = aiResponse.Substring(lowerResponse.IndexOf("tips:") + 5);
        }
        else if (lowerResponse.Contains("actionable"))
        {
            var actionableIndex = lowerResponse.IndexOf("actionable");
            tipsSection = aiResponse.Substring(actionableIndex);
        }

        if (!string.IsNullOrWhiteSpace(tipsSection))
        {
            var lines = tipsSection.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var cleanLine = line.Trim()
                    .TrimStart('-', '*', '•', '1', '2', '3', '4', '5', '.', ' ')
                    .Trim();

                if (!string.IsNullOrWhiteSpace(cleanLine) &&
                    cleanLine.Length > 15 &&
                    cleanLine.Length < 150 &&
                    !cleanLine.StartsWith("Quote", StringComparison.OrdinalIgnoreCase))
                {
                    tips.Add(cleanLine);
                    if (tips.Count >= 3) break; // Maximal 3 Tips
                }
            }
        }

        return tips.Any() ? tips : null;
    }

    private string GetFallbackMotivation(MotivationRequestDto request)
    {
        var athleteName = request.AthleteProfile?.Name ?? "Champion";
        var primaryGoal = request.AthleteProfile?.PrimaryGoal ?? "fitness goals";
        var fitnessLevel = request.AthleteProfile?.FitnessLevel ?? "current";

        var motivations = new[]
        {
            $"Great job, {athleteName}! Your consistency in training is inspiring. Every workout brings you closer to your {primaryGoal}.",
            $"You're making excellent progress, {athleteName}! Your dedication to fitness shows real commitment to your health and goals.",
            $"Keep pushing forward, {athleteName}! Your {fitnessLevel} level shows you have what it takes to achieve great things.",
            $"Amazing work, {athleteName}! Your commitment to {primaryGoal} is paying off. Stay strong and keep moving forward!"
        };

        var random = new Random();
        return motivations[random.Next(motivations.Length)];
    }
}