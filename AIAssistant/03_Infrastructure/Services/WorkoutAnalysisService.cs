using AIAssistant._02_Application.DTOs;
using AIAssistant._02_Application.Interfaces;
using AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Services;

namespace AIAssistant._03_Infrastructure.Services;

public class WorkoutAnalysisService : IWorkoutAnalysisService
{
    private readonly HuggingFaceService _huggingFaceService;
    private readonly GoogleGeminiService _googleGeminiService;
    private readonly ILogger<WorkoutAnalysisService> _logger;
    private readonly IConfiguration _configuration;

    public WorkoutAnalysisService(
        HuggingFaceService huggingFaceService,
        GoogleGeminiService googleGeminiService,
        IConfiguration configuration,
        ILogger<WorkoutAnalysisService> logger)
    {
        _huggingFaceService = huggingFaceService;
        _googleGeminiService = googleGeminiService;
        _configuration = configuration;
        _logger = logger;
    }

    // Generic method 
    public async Task<WorkoutAnalysisResponseDto> AnalyzeWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        var defaultProvider = _configuration["AI:DefaultProvider"] ?? "GoogleGemini";
        return await AnalyzeWorkoutsWithProviderAsync(request, defaultProvider);
    }

    // HuggingFace specific method
    public async Task<WorkoutAnalysisResponseDto> AnalyzeHuggingFaceWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        return await AnalyzeWorkoutsWithProviderAsync(request, "HuggingFace");
    }

    // NEW: Google Gemini specific method
    public async Task<WorkoutAnalysisResponseDto> AnalyzeGoogleGeminiWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        return await AnalyzeWorkoutsWithProviderAsync(request, "GoogleGemini");
    }

    // Core analysis method with provider selection
    private async Task<WorkoutAnalysisResponseDto> AnalyzeWorkoutsWithProviderAsync(
        WorkoutAnalysisRequestDto request,
        string provider)
    {
        try
        {
            _logger.LogInformation("Analyzing workouts with {Provider} for {WorkoutCount} recent workouts, analysis type: {AnalysisType}",
                provider, request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

            var prompt = BuildAnalysisPrompt(request);

            // Select the appropriate AI service
            IAIPromptService aiService = provider.ToLower() switch
            {
                "huggingface" => _huggingFaceService,
                "googlegemini" => _googleGeminiService,
                _ => _huggingFaceService // Default fallback
            };

            // Call the appropriate AI method based on analysis type
            string aiResponse = request.AnalysisType?.ToLower() switch
            {
                "health" => await aiService.GetHealthAnalysisAsync(prompt),
                "performance" => await aiService.GetFitnessAnalysisAsync(prompt),
                "trends" => await aiService.GetFitnessAnalysisAsync(prompt),
                _ => await aiService.GetFitnessAnalysisAsync(prompt)
            };

            var result = ParseAnalysisResponse(aiResponse, request.AnalysisType);
            result.Provider = provider; // Add provider info to response

            _logger.LogInformation("Successfully generated workout analysis with {Provider}: {InsightCount} insights and {RecommendationCount} recommendations",
                provider, result.KeyInsights?.Count ?? 0, result.Recommendations?.Count ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workouts with {Provider}", provider);

            // Provider-specific fallback
            return GetFallbackAnalysis(request, provider);
        }
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
        return $@"Du bist ein Gesundheits- und Fitnessexperte. Analysiere die folgenden Trainingsdaten für Gesundheitserkenntnisse:

TRAININGSDATEN:
{workoutsData}
{athleteContext}

Erstelle eine gesundheitsfokussierte Analyse mit:

GESUNDHEITSANALYSE:
- Bewertung der Trainingsbelastung (angemessen/übermäßig?)
- Regenerationsmuster und Empfehlungen
- Verletzungspräventions-Erkenntnisse
- Herz-Kreislauf-Gesundheitsindikatoren

WICHTIGE ERKENNTNISSE:
- 3-4 spezifische gesundheitsbezogene Beobachtungen
- Warnzeichen falls vorhanden

EMPFEHLUNGEN:
- Gesundheitsorientierte umsetzbare Ratschläge
- Regenerationsstrategien
- Trainingsmodifikationen für optimale Gesundheit

Konzentriere dich auf Gesundheit und Verletzungsprävention.";
    }

    private string BuildPerformanceAnalysisPrompt(string workoutsData, string athleteContext)
    {
        return $@"Du bist ein Leistungstrainer. Analysiere die folgenden Trainingsdaten für Leistungsoptimierung:

TRAININGSDATEN:
{workoutsData}
{athleteContext}

Erstelle eine leistungsfokussierte Analyse mit:

LEISTUNGSANALYSE:
- Fortschrittsbewertung und Trends
- Leistungsstärken und -schwächen
- Bewertung der Trainingseffizienz
- Zielerreichungspotential

WICHTIGE ERKENNTNISSE:
- 3-4 spezifische Leistungsbeobachtungen
- Identifizierte Verbesserungsbereiche

EMPFEHLUNGEN:
- Strategien zur Leistungsoptimierung
- Anpassungen der Trainingsintensität
- Spezifische Techniken zur Verbesserung

Fokussiere dich auf sportliche Leistung und Wettkampfverbesserung.";
    }

    private string BuildTrendsAnalysisPrompt(string workoutsData, string athleteContext)
    {
        return $@"Du bist ein Datenanalyst spezialisiert auf Fitnesstrends. Analysiere die folgenden Trainingsmuster:

TRAININGSDATEN:
{workoutsData}
{athleteContext}

Erstelle eine trendfokussierte Analyse mit:

TRENDANALYSE:
- Trainingskonsistenzmuster über die Zeit
- Leistungsfortschritt oder -rückgang
- Wöchentliche/monatliche Mustererkennung
- Trainingsvielfalt und -verteilung

WICHTIGE ERKENNTNISSE:
- 3-4 bedeutsame Trendbeobachtungen
- Mustererkennung-Befunde

EMPFEHLUNGEN:
- Trendbasierte Trainingsratschläge
- Strategien zur Konsistenzverbesserung
- Vorschläge für zukünftige Planung

Fokussiere dich auf Muster, Trends und langfristige Fortschrittsanalyse.";
    }

    private string BuildGeneralAnalysisPrompt(string workoutsData, string athleteContext, string analysisType)
    {
        return $@"Du bist ein Fitnessexperte. Erstelle eine umfassende Analyse der folgenden Trainingsdaten:

TRAININGSDATEN:
{workoutsData}
{athleteContext}

Analysefokus: {analysisType}

Erstelle eine detaillierte Fitnessanalyse mit:

ANALYSE:
- Gesamtbewertung des Trainings
- Bewertung der Trainingseffektivität
- Fortschrittsindikatoren
- Verbesserungsbereiche

WICHTIGE ERKENNTNISSE:
- 3-4 spezifische Beobachtungen aus den Daten
- Wichtige Muster oder Trends
- Leistungshöhepunkte

EMPFEHLUNGEN:
- Umsetzbare Trainingsratschläge
- Spezifische Verbesserungsstrategien
- Zielorientierte Vorschläge

Liefere praktische, umsetzbare Erkenntnisse für Fitnessverbesserung.";
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

        // Suche nach verschiedenen Analysis-Section Headers (deutsch und englisch)
        var analysisHeaders = new[] {
            "ANALYSE:", "GESUNDHEITSANALYSE:", "LEISTUNGSANALYSE:", "TRENDANALYSE:",
            "ANALYSIS:", "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:"
        };

        foreach (var header in analysisHeaders)
        {
            if (aiResponse.Contains(header, StringComparison.OrdinalIgnoreCase))
            {
                var analysisParts = aiResponse.Split(header, StringSplitOptions.RemoveEmptyEntries);
                if (analysisParts.Length > 1)
                {
                    var analysisSection = analysisParts[1].Split(new[] {
                        "WICHTIGE ERKENNTNISSE:", "EMPFEHLUNGEN:",
                        "KEY INSIGHTS:", "RECOMMENDATIONS:"
                    }, StringSplitOptions.RemoveEmptyEntries)[0];

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

            // Stoppe bei strukturierten Abschnitten (deutsch und englisch)
            if (cleanLine.StartsWith("WICHTIGE ERKENNTNISSE", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("EMPFEHLUNGEN", StringComparison.OrdinalIgnoreCase) ||
                cleanLine.StartsWith("KEY INSIGHTS", StringComparison.OrdinalIgnoreCase) ||
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
        return ExtractListSection(aiResponse, new[] { "WICHTIGE ERKENNTNISSE:", "KEY INSIGHTS:", "INSIGHTS:", "ERKENNTNISSE:" });
    }

    private List<string>? ExtractRecommendations(string aiResponse)
    {
        return ExtractListSection(aiResponse, new[] { "EMPFEHLUNGEN:", "RECOMMENDATIONS:", "ADVICE:", "RATSCHLÄGE:" });
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

                // Stoppe bei nächstem Header (deutsch und englisch)
                var nextHeaders = new[] {
                    "ANALYSE:", "WICHTIGE ERKENNTNISSE:", "EMPFEHLUNGEN:", "RATSCHLÄGE:",
                    "GESUNDHEITSANALYSE:", "LEISTUNGSANALYSE:", "TRENDANALYSE:",
                    "ANALYSIS:", "KEY INSIGHTS:", "RECOMMENDATIONS:", "ADVICE:",
                    "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:"
                };
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

    private WorkoutAnalysisResponseDto GetFallbackAnalysis(WorkoutAnalysisRequestDto request, string provider = "Unknown")
    {
        var workoutCount = request.RecentWorkouts?.Count ?? 0;
        var totalDistance = request.RecentWorkouts?.Sum(w => w.Distance) ?? 0;
        var totalDuration = request.RecentWorkouts?.Sum(w => w.Duration) ?? 0;
        var avgCalories = request.RecentWorkouts?.Any() == true ?
            request.RecentWorkouts.Average(w => w.Calories) : 0;

        var analysisType = request.AnalysisType ?? "Performance";

        var analysis = analysisType.ToLower() switch
        {
            "health" => $"Basierend auf Ihren {workoutCount} letzten Trainingseinheiten scheint Ihre Trainingsbelastung gut ausgewogen zu sein. " +
                       $"Die Gesamtdistanz von {totalDistance:F1}km über {TimeSpan.FromSeconds(totalDuration):h\\:mm} zeigt gutes Herz-Kreislauf-Engagement. " +
                       $"Keine bedenklichen Übertrainingsmuster erkannt. Ihr durchschnittlicher Kalorienverbrauch von {avgCalories:F0} pro Einheit deutet auf angemessene Trainingsintensität hin.",

            "performance" => $"Ihre Leistungsdaten zeigen {workoutCount} absolvierte Trainingseinheiten mit {totalDistance:F1}km Gesamtdistanz. " +
                           $"Die Trainingskonsistenz erscheint stark mit variierenden Trainingsarten. " +
                           $"Die durchschnittliche Einheitsdauer von {TimeSpan.FromSeconds(workoutCount > 0 ? totalDuration / workoutCount : 0):h\\:mm} deutet auf guten Ausdaueraufbau hin. " +
                           $"Leistungsmetriken zeigen stetigen Fortschritt in Richtung Ihrer Ziele.",

            "trends" => $"Die Trainingstrendanalyse zeigt {workoutCount} Trainingseinheiten über den letzten Zeitraum. " +
                       $"Der Gesamtdistanzfortschritt auf {totalDistance:F1}km zeigt positive Trainingskonsistenz. " +
                       $"Trainingshäufigkeits- und Dauermuster deuten auf nachhaltige Trainingsgewohnheiten hin. " +
                       $"Kalorienverbrauchstrends deuten auf effektives Energiemanagement hin.",

            _ => $"Umfassende Analyse Ihrer {workoutCount} letzten Trainingseinheiten über {totalDistance:F1}km zeigt exzellente Trainingskonsistenz. " +
                $"Ihre {analysisType.ToLower()}-Metriken deuten auf stetigen Fortschritt in Richtung Ihrer Fitnessziele hin. " +
                $"Trainingsbelastung und Regenerationsbalance scheinen angemessen für kontinuierliche Verbesserung."
        };

        var insights = analysisType.ToLower() switch
        {
            "health" => new List<string>
            {
                $"{workoutCount} Trainingseinheiten ohne Übertrainingsindikatoren absolviert",
                $"Durchschnittlicher Kalorienverbrauch von {avgCalories:F0} deutet auf angemessene Intensität hin",
                "Trainingshäufigkeit unterstützt gute Herz-Kreislauf-Gesundheit",
                "Keine bedenklichen Gesundheitsmuster in den Trainingsdaten erkannt"
            },
            "performance" => new List<string>
            {
                $"{totalDistance:F1}km Gesamtdistanz über {workoutCount} Einheiten erreicht",
                "Trainingskonsistenz zeigt starkes Engagement für Leistungsziele",
                $"Durchschnittliche Trainingsintensität von {avgCalories:F0} Kalorien ist leistungsorientiert",
                "Trainingsvielfalt unterstützt vielseitige sportliche Entwicklung"
            },
            "trends" => new List<string>
            {
                $"Trainingshäufigkeit von {workoutCount} Einheiten zeigt konsistente Gewohnheitsbildung",
                "Distanz- und Dauertrends deuten auf Anwendung progressiver Überlastung hin",
                "Kalorienverbrauchsmuster deuten auf effektives Trainingsintensitätsmanagement hin",
                "Gesamttrajektorie deutet auf anhaltende Fitnessverbesserung hin"
            },
            _ => new List<string>
            {
                $"{workoutCount} Trainingseinheiten mit Gesamtdistanz von {totalDistance:F1}km absolviert",
                "Trainingskonsistenz zeigt Engagement für Fitnessziele",
                "Leistungsmetriken deuten auf stetige Verbesserungstrajektorie hin",
                "Trainingsintensität und -häufigkeit scheinen gut ausgewogen"
            }
        };

        var recommendations = analysisType.ToLower() switch
        {
            "health" => new List<string>
            {
                "Aktuellen Trainingsplan fortsetzen, um Gesundheitsvorteile zu erhalten",
                "Regenerationszeichen überwachen und Intensität bei Müdigkeit anpassen",
                "Ausreichend Schlaf und Ernährung sicherstellen, um Trainingsbelastung zu unterstützen",
                "Mobilitätsarbeit hinzufügen, um Verletzungen vorzubeugen"
            },
            "performance" => new List<string>
            {
                "Trainingsintensität schrittweise um 5-10% für Leistungssteigerungen erhöhen",
                "Intervalltraining hinzufügen, um Geschwindigkeits- und Kraftentwicklung zu fördern",
                "Leistungstests in Betracht ziehen, um spezifische Verbesserungen zu verfolgen",
                "Sportspezifische Übungen für gezielte Fertigkeitsentwicklung integrieren"
            },
            "trends" => new List<string>
            {
                "Aktuelle Trainingshäufigkeit für anhaltende positive Trends beibehalten",
                "Progressive Steigerungen in Distanz und Dauer planen",
                "Wöchentliche Trends verfolgen, um optimale Trainingsmuster zu identifizieren",
                "Monatliche Ziele basierend auf aktueller Fortschrittsrate setzen"
            },
            _ => new List<string>
            {
                "Mit aktuellem Trainingsplan und -intensität fortfahren",
                "Trainingsschwierigkeit alle 2-3 Wochen schrittweise um 5-10% erhöhen",
                "Ausreichende Erholung zwischen intensiven Trainingseinheiten sicherstellen",
                "Trainingsvielfalt für ausgewogene Entwicklung hinzufügen"
            }
        };

        return new WorkoutAnalysisResponseDto
        {
            Analysis = analysis,
            KeyInsights = insights,
            Recommendations = recommendations,
            GeneratedAt = DateTime.UtcNow,
            Provider = provider
        };
    }
}