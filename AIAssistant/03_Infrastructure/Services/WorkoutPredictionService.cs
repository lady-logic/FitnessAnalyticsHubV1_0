using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;

namespace AIAssistant._03_Infrastructure.Services
{
    public class WorkoutPredictionService : IWorkoutPredictionService
    {
        private readonly IAIPromptService _aiPromptService;
        private readonly ILogger<WorkoutPredictionService> _logger;

        public WorkoutPredictionService(
            IAIPromptService aiPromptService,
            ILogger<WorkoutPredictionService> logger)
        {
            _aiPromptService = aiPromptService;
            _logger = logger;
        }

        public async Task<WorkoutPredictionResponseDto> PredictOpenAIWorkoutPerformanceAsync(
            WorkoutPredictionRequestDto request)
        {
            try
            {
                // Erstelle einen Prompt für die Prognose
                var prompt = BuildPredictionPrompt(request);

                // Rufe die OpenAI API auf
                var aiResponse = await _aiPromptService.GetOpenAICompletionAsync(prompt);

                // Parse und strukturiere die Antwort
                return ParsePredictionResponse(aiResponse, request.TargetWorkoutType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting workout performance");
                throw;
            }
        }

        public async Task<WorkoutPredictionResponseDto> PredictClaudeWorkoutPerformanceAsync(
            WorkoutPredictionRequestDto request)
        {
            try
            {
                // Erstelle einen Prompt für die Prognose
                var prompt = BuildPredictionPrompt(request);

                // Rufe die OpenAI API auf
                var aiResponse = await _aiPromptService.GetClaudeCompletionAsync(prompt);

                // Parse und strukturiere die Antwort
                return ParsePredictionResponse(aiResponse, request.TargetWorkoutType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting workout performance");
                throw;
            }
        }

        private string BuildPredictionPrompt(WorkoutPredictionRequestDto request)
        {
            // Formatiere die vergangenen Workout-Daten für den Prompt
            var pastWorkoutsData = string.Join("\n", request.PastWorkouts
                .OrderByDescending(w => w.Date)
                .Take(5) // Die letzten 5 Workouts sollten ausreichen
                .Select(w =>
                    $"Date: {w.Date:yyyy-MM-dd}, Type: {w.ActivityType}, Distance: {w.Distance}km, " +
                    $"Duration: {TimeSpan.FromSeconds(w.Duration):hh\\:mm\\:ss}, Pace: {w.Duration / 60.0 / (w.Distance / 1000):F2} min/km"
                ));

            // Informationen über das Zieltraining
            var goalInfo = !string.IsNullOrEmpty(request.Goal) ?
                $"Goal: {request.Goal}" : "No specific goal provided";

            return $@"As a fitness analytics expert, predict the performance for an upcoming workout based on past workout data:

Past Workouts:
{pastWorkoutsData}

Target Workout Type: {request.TargetWorkoutType}
Target Date: {request.TargetDate:yyyy-MM-dd}
{goalInfo}

Based on the pattern of the athlete's past performances, please predict:
1. Expected distance in km
2. Expected duration in hours:minutes:seconds
3. Expected pace in minutes per km
4. Estimated calories burned
5. Brief explanation of the prediction basis
6. 2-3 preparation tips for optimal performance

Format your response with clear numerical predictions and explanations.";
        }

        private WorkoutPredictionResponseDto ParsePredictionResponse(
            string aiResponse, string workoutType)
        {
            var response = new WorkoutPredictionResponseDto
            {
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                // Extrahiere die prognostizierte Distanz
                var distanceMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:distance|Distance)[:\s]+(\d+\.?\d*)\s*km");
                if (distanceMatch.Success)
                {
                    response.PredictedDistance = double.Parse(distanceMatch.Groups[1].Value);
                }
                else
                {
                    // Fallback: Bei Run/Laufen standardmäßig 5km, bei Ride/Radfahren 20km
                    response.PredictedDistance = workoutType.Contains("Run") ? 5.0 : 20.0;
                }

                // Extrahiere die prognostizierte Dauer
                var durationMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:duration|Duration)[:\s]+(?:(\d+):)?(\d+):(\d+)");
                if (durationMatch.Success)
                {
                    int hours = 0;
                    if (!string.IsNullOrEmpty(durationMatch.Groups[1].Value))
                    {
                        hours = int.Parse(durationMatch.Groups[1].Value);
                    }
                    int minutes = int.Parse(durationMatch.Groups[2].Value);
                    int seconds = int.Parse(durationMatch.Groups[3].Value);

                    response.PredictedDuration = hours * 3600 + minutes * 60 + seconds;
                }
                else
                {
                    // Fallback: Bei Run/Laufen standardmäßig 30 Minuten, bei Ride/Radfahren 60 Minuten
                    response.PredictedDuration = workoutType.Contains("Run") ? 1800 : 3600;
                }

                // Extrahiere das prognostizierte Tempo
                var paceMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:pace|Pace)[:\s]+(\d+\.?\d*)\s*min\/km");
                if (paceMatch.Success)
                {
                    response.PredictedPace = double.Parse(paceMatch.Groups[1].Value);
                }
                else
                {
                    // Fallback: Berechne aus Distanz und Dauer
                    response.PredictedPace = response.PredictedDuration / 60.0 / response.PredictedDistance;
                }

                // Extrahiere die prognostizierten Kalorien
                var caloriesMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:calories|Calories)[:\s]+(\d+)");
                if (caloriesMatch.Success)
                {
                    response.PredictedCalories = int.Parse(caloriesMatch.Groups[1].Value);
                }
                else
                {
                    // Fallback: Grobe Schätzung basierend auf Aktivitätstyp und Dauer
                    var caloriesPerMinute = workoutType.Contains("Run") ? 10 : 8;
                    response.PredictedCalories = (int)(response.PredictedDuration / 60.0 * caloriesPerMinute);
                }

                // Extrahiere die Erklärung
                var explanationMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:explanation|Explanation)[:\s]+([\s\S]+?)(?=\s*(?:Tips|Preparation|$))");
                if (explanationMatch.Success)
                {
                    response.Explanation = explanationMatch.Groups[1].Value.Trim();
                }

                // Extrahiere die Tipps
                var tipsMatch = System.Text.RegularExpressions.Regex.Match(
                    aiResponse, @"(?:tips|Tips|preparation|Preparation)[:\s]+([\s\S]+)$");
                if (tipsMatch.Success)
                {
                    response.PreparationTips = tipsMatch.Groups[1].Value
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Where(line => !string.IsNullOrWhiteSpace(line) &&
                               (line.TrimStart().StartsWith("-") || line.TrimStart().StartsWith("*") || char.IsDigit(line.TrimStart()[0])))
                        .Select(line => line.TrimStart('-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Bei Parsing-Fehlern setzen wir die Erklärung auf eine Fehlermeldung
                response.Explanation = "Error parsing AI response. Using default values.";
            }

            return response;
        }
    }
}
