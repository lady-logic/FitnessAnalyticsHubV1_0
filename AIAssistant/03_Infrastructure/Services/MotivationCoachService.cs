using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;

namespace AIAssistant._03_Infrastructure.Services
{
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

        public async Task<MotivationResponseDto> GetOpenAIMotivationalMessageAsync(
            MotivationRequestDto request)
        {
            try
            {
                // Erstelle einen Prompt für die Motivation
                var prompt = BuildMotivationPrompt(request);

                // Rufe die OpenAI API auf
                var aiResponse = await _aiPromptService.GetOpenAICompletionAsync(prompt);

                // Parse und strukturiere die Antwort
                return ParseMotivationResponse(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating motivational message");
                throw;
            }
        }

        public async Task<MotivationResponseDto> GetClaudeMotivationalMessageAsync(
            MotivationRequestDto request)
        {
            try
            {
                // Erstelle einen Prompt für die Motivation
                var prompt = BuildMotivationPrompt(request);

                // Rufe die Claude API auf
                var aiResponse = await _aiPromptService.GetClaudeCompletionAsync(prompt);

                // Parse und strukturiere die Antwort
                return ParseMotivationResponse(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating motivational message");
                throw;
            }
        }

        private string BuildMotivationPrompt(MotivationRequestDto request)
        {
            // Informationen zum letzten Training (wenn vorhanden)
            var lastWorkoutInfo = request.LastWorkout != null ?
                $"Last workout: {request.LastWorkout.ActivityType}, {request.LastWorkout.Distance}km, {TimeSpan.FromSeconds(request.LastWorkout.Duration):hh\\:mm\\:ss}" :
                "No recent workout data available";

            // Informationen über das anstehende Training (wenn vorhanden)
            var upcomingWorkoutInfo = !string.IsNullOrEmpty(request.UpcomingWorkoutType) ?
                $"Upcoming workout: {request.UpcomingWorkoutType}" : "";

            // Motivationslevel anpassen
            var motivationLevel = request.IsStruggling ?
                "The athlete is currently struggling with motivation." :
                "The athlete is looking for additional motivation.";

            return $@"As a motivational fitness coach, create a personalized motivational message for an athlete with the following profile:

Name: {request.AthleteProfile.Name}
Fitness Level: {request.AthleteProfile.FitnessLevel}
Primary Goal: {request.AthleteProfile.PrimaryGoal}
{lastWorkoutInfo}
{upcomingWorkoutInfo}
{motivationLevel}

Please provide:
1. A short, personalized motivational message (2-3 sentences)
2. An inspirational quote related to fitness or perseverance
3. 1-2 actionable tips for staying motivated

Keep the tone positive, encouraging, and specific to the athlete's situation.";
        }

        private MotivationResponseDto ParseMotivationResponse(string aiResponse)
        {
            var response = new MotivationResponseDto
            {
                MotivationalMessage = aiResponse,
                GeneratedAt = DateTime.UtcNow
            };

            // Extrahiere das Zitat, falls vorhanden
            if (aiResponse.Contains("Quote:") || aiResponse.Contains("\""))
            {
                var quoteMatch = System.Text.RegularExpressions.Regex.Match(aiResponse, @"""(.+?)""");
                if (quoteMatch.Success)
                {
                    response.Quote = quoteMatch.Groups[1].Value;
                }
                else if (aiResponse.Contains("Quote:"))
                {
                    var quoteParts = aiResponse.Split("Quote:");
                    if (quoteParts.Length > 1)
                    {
                        var quoteLine = quoteParts[1].Split('\n')[0].Trim();
                        response.Quote = quoteLine;
                    }
                }
            }

            // Extrahiere die Tipps, falls vorhanden
            if (aiResponse.Contains("Tips:") || aiResponse.Contains("Tip:"))
            {
                var tipsSection = aiResponse.Contains("Tips:") ?
                    aiResponse.Split("Tips:")[1] :
                    aiResponse.Split("Tip:")[1];

                response.ActionableTips = tipsSection
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line) &&
                           (line.TrimStart().StartsWith("-") || line.TrimStart().StartsWith("*") || char.IsDigit(line.TrimStart()[0])))
                    .Select(line => line.TrimStart('-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
                    .ToList();
            }

            return response;
        }
    }
}
