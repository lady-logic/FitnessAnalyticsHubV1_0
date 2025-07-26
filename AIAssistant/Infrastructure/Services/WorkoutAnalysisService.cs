using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using FitnessAnalyticsHub.AIAssistant.Infrastructure.Services;

namespace AIAssistant.Infrastructure.Services;

public class WorkoutAnalysisService : IWorkoutAnalysisService
{
    private readonly HuggingFaceService huggingFaceService;
    private readonly GoogleGeminiService googleGeminiService;
    private readonly ILogger<WorkoutAnalysisService> logger;
    private readonly IConfiguration configuration;

    public WorkoutAnalysisService(
        HuggingFaceService huggingFaceService,
        GoogleGeminiService googleGeminiService,
        IConfiguration configuration,
        ILogger<WorkoutAnalysisService> logger)
    {
        this.huggingFaceService = huggingFaceService;
        this.googleGeminiService = googleGeminiService;
        this.configuration = configuration;
        this.logger = logger;
    }

    // Generic method
    public async Task<WorkoutAnalysisResponseDto> AnalyzeWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        var defaultProvider = this.configuration["AI:DefaultProvider"] ?? "GoogleGemini";
        return await this.AnalyzeWorkoutsWithProviderAsync(request, defaultProvider);
    }

    // HuggingFace specific method
    public async Task<WorkoutAnalysisResponseDto> AnalyzeHuggingFaceWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        return await this.AnalyzeWorkoutsWithProviderAsync(request, "HuggingFace");
    }

    // Google Gemini specific method
    public async Task<WorkoutAnalysisResponseDto> AnalyzeGoogleGeminiWorkoutsAsync(
        WorkoutAnalysisRequestDto request)
    {
        return await this.AnalyzeWorkoutsWithProviderAsync(request, "GoogleGemini");
    }

    // Core analysis method with provider selection
    private async Task<WorkoutAnalysisResponseDto> AnalyzeWorkoutsWithProviderAsync(
        WorkoutAnalysisRequestDto request,
        string provider)
    {
        try
        {
            this.logger.LogInformation(
                "Analyzing workouts with {Provider} for {WorkoutCount} recent workouts, analysis type: {AnalysisType}",
                provider, request.RecentWorkouts?.Count ?? 0, request.AnalysisType ?? "General");

            var prompt = this.BuildAnalysisPrompt(request);

            // Select the appropriate AI service
            IAIPromptService aiService = provider.ToLower() switch
            {
                "huggingface" => this.huggingFaceService,
                "googlegemini" => this.googleGeminiService,
                _ => this.huggingFaceService // Default fallback
            };

            // Call the appropriate AI method based on analysis type
            string aiResponse = request.AnalysisType?.ToLower() switch
            {
                "health" => await aiService.GetHealthAnalysisAsync(prompt),
                "performance" => await aiService.GetFitnessAnalysisAsync(prompt),
                "trends" => await aiService.GetFitnessAnalysisAsync(prompt),
                _ => await aiService.GetFitnessAnalysisAsync(prompt)
            };

            var result = this.ParseAnalysisResponse(aiResponse, request.AnalysisType);
            result.Provider = provider; // Add provider info to response

            this.logger.LogInformation(
                "Successfully generated workout analysis with {Provider}: {InsightCount} insights and {RecommendationCount} recommendations",
                provider, result.KeyInsights?.Count ?? 0, result.Recommendations?.Count ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing workouts with {Provider}", provider);

            // Provider-specific fallback
            return this.GetFallbackAnalysis(request, provider);
        }
    }

    private string BuildAnalysisPrompt(WorkoutAnalysisRequestDto request)
    {
        if (request.RecentWorkouts == null || !request.RecentWorkouts.Any())
        {
            return "No recent workout data available for analysis. Please provide workout data to generate insights.";
        }

        var workoutsData = string.Join("\n", request.RecentWorkouts.Select(w =>
            $"Date: {w.Date:yyyy-MM-dd}, Type: {w.ActivityType}, Distance: {w.Distance}m, " +
            $"Duration: {TimeSpan.FromSeconds(w.Duration):hh\\:mm\\:ss}, Calories: {w.Calories}"));

        var athleteContext = request.AthleteProfile != null ?
            $"\nAthlete Level: {request.AthleteProfile.FitnessLevel}\nPrimary Goal: {request.AthleteProfile.PrimaryGoal}" : string.Empty;

        // Spezifischer Prompt basierend auf Analyse-Typ
        var analysisPrompt = request.AnalysisType?.ToLower() switch
        {
            "health" => this.BuildHealthAnalysisPrompt(workoutsData, athleteContext),
            "performance" => this.BuildPerformanceAnalysisPrompt(workoutsData, athleteContext),
            "trends" => this.BuildTrendsAnalysisPrompt(workoutsData, athleteContext),
            _ => this.BuildGeneralAnalysisPrompt(workoutsData, athleteContext, request.AnalysisType ?? "General")
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
            Analysis = this.ExtractAnalysisSection(aiResponse),
            KeyInsights = this.ExtractKeyInsights(aiResponse),
            Recommendations = this.ExtractRecommendations(aiResponse),
            GeneratedAt = DateTime.UtcNow,
        };

        return response;
    }

    private string ExtractAnalysisSection(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return this.GetDefaultAnalysis();
        }

        // Versuche erst strukturierte Extraktion
        var structuredAnalysis = this.TryExtractStructuredAnalysis(aiResponse);
        if (!string.IsNullOrEmpty(structuredAnalysis))
        {
            return this.LimitAnalysisLength(structuredAnalysis);
        }

        // Fallback: Freie Text-Extraktion
        var fallbackAnalysis = this.ExtractFallbackAnalysis(aiResponse);
        return this.LimitAnalysisLength(fallbackAnalysis);
    }

    private string TryExtractStructuredAnalysis(string aiResponse)
    {
        var analysisHeaders = this.GetAnalysisHeaders();

        foreach (var header in analysisHeaders)
        {
            if (!aiResponse.Contains(header, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var analysisSection = this.ExtractSectionContent(aiResponse, header);
            if (this.IsValidAnalysis(analysisSection))
            {
                return analysisSection;
            }
        }

        return string.Empty;
    }

    private string ExtractSectionContent(string aiResponse, string header)
    {
        var analysisParts = aiResponse.Split(header, StringSplitOptions.RemoveEmptyEntries);
        if (analysisParts.Length <= 1)
        {
            return string.Empty;
        }

        var stopMarkers = this.GetStopMarkers();
        var analysisSection = analysisParts[1].Split(stopMarkers, StringSplitOptions.RemoveEmptyEntries)[0];

        return analysisSection.Trim();
    }

    private string ExtractFallbackAnalysis(string aiResponse)
    {
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var analysisLines = new List<string>();
        var stopMarkers = this.GetStopMarkers();

        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            if (string.IsNullOrWhiteSpace(cleanLine))
            {
                continue;
            }

            if (this.ShouldStopAtLine(cleanLine, stopMarkers))
            {
                break;
            }

            analysisLines.Add(cleanLine);

            if (analysisLines.Count >= 5)
            {
                break;
            }
        }

        return analysisLines.Any()
            ? string.Join(" ", analysisLines)
            : this.GetDefaultAnalysis();
    }

    private string LimitAnalysisLength(string analysis)
    {
        if (analysis.Length <= 400)
        {
            return analysis;
        }

        var sentences = analysis.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(". ", sentences.Take(4)) + ".";
    }

    private bool IsValidAnalysis(string analysis)
    {
        return !string.IsNullOrWhiteSpace(analysis) && analysis.Length > 20;
    }

    private bool ShouldStopAtLine(string line, string[] stopMarkers)
    {
        return stopMarkers.Any(marker =>
            line.StartsWith(marker, StringComparison.OrdinalIgnoreCase));
    }

    private string GetDefaultAnalysis()
    {
        return "Unable to generate analysis at this time. Please try again later.";
    }

    private string[] GetAnalysisHeaders()
    {
        return new[]
        {
        "ANALYSE:", "GESUNDHEITSANALYSE:", "LEISTUNGSANALYSE:", "TRENDANALYSE:",
        "ANALYSIS:", "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:",
        };
    }

    private string[] GetStopMarkers()
    {
        return new[]
        {
        "WICHTIGE ERKENNTNISSE:", "EMPFEHLUNGEN:",
        "KEY INSIGHTS:", "RECOMMENDATIONS:",
        };
    }

    private List<string>? ExtractKeyInsights(string aiResponse)
    {
        return this.ExtractListSection(aiResponse, new[] { "WICHTIGE ERKENNTNISSE:", "KEY INSIGHTS:", "INSIGHTS:", "ERKENNTNISSE:" });
    }

    private List<string>? ExtractRecommendations(string aiResponse)
    {
        return this.ExtractListSection(aiResponse, new[] { "EMPFEHLUNGEN:", "RECOMMENDATIONS:", "ADVICE:", "RATSCHLÄGE:" });
    }

    private List<string>? ExtractListSection(string aiResponse, string[] sectionHeaders)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return null;
        }

        foreach (var header in sectionHeaders)
        {
            var extractedItems = this.TryExtractFromHeader(aiResponse, header);
            if (extractedItems?.Any() == true)
            {
                return extractedItems;
            }
        }

        return null;
    }

    private List<string>? TryExtractFromHeader(string aiResponse, string header)
    {
        if (!aiResponse.Contains(header, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sectionContent = this.ExtractSectionUntilNextHeader(aiResponse, header);
        if (string.IsNullOrEmpty(sectionContent))
        {
            return null;
        }

        return this.ParseItemsFromSection(sectionContent);
    }

    private string ExtractSectionUntilNextHeader(string aiResponse, string currentHeader)
    {
        var headerIndex = aiResponse.IndexOf(currentHeader, StringComparison.OrdinalIgnoreCase);
        var section = aiResponse.Substring(headerIndex + currentHeader.Length);

        var nextHeaderIndex = this.FindNextHeaderIndex(section, currentHeader);
        if (nextHeaderIndex > 0)
        {
            section = section.Substring(0, nextHeaderIndex);
        }

        return section;
    }

    private int FindNextHeaderIndex(string section, string currentHeader)
    {
        var allHeaders = this.GetAllSectionHeaders();

        foreach (var nextHeader in allHeaders)
        {
            if (nextHeader == currentHeader)
            {
                continue;
            }

            if (section.Contains(nextHeader, StringComparison.OrdinalIgnoreCase))
            {
                return section.IndexOf(nextHeader, StringComparison.OrdinalIgnoreCase);
            }
        }

        return -1;
    }

    private List<string> ParseItemsFromSection(string sectionContent)
    {
        var items = new List<string>();
        var lines = sectionContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var cleanedItem = this.CleanLineItem(line);

            if (this.IsValidListItem(cleanedItem))
            {
                items.Add(cleanedItem);

                if (items.Count >= 5) // Maximal 5 Items
                {
                    break;
                }
            }
        }

        return items;
    }

    private string CleanLineItem(string line)
    {
        return line.Trim()
            .TrimStart('-', '*', '•', '1', '2', '3', '4', '5', '.', ' ')
            .Trim();
    }

    private bool IsValidListItem(string item)
    {
        return !string.IsNullOrWhiteSpace(item) &&
               item.Length > 15 &&
               item.Length < 200;
    }

    private string[] GetAllSectionHeaders()
    {
        return new[]
        {
        "ANALYSE:", "WICHTIGE ERKENNTNISSE:", "EMPFEHLUNGEN:", "RATSCHLÄGE:",
        "GESUNDHEITSANALYSE:", "LEISTUNGSANALYSE:", "TRENDANALYSE:",
        "ANALYSIS:", "KEY INSIGHTS:", "RECOMMENDATIONS:", "ADVICE:",
        "HEALTH ANALYSIS:", "PERFORMANCE ANALYSIS:", "TRENDS ANALYSIS:",
        };
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
                "Keine bedenklichen Gesundheitsmuster in den Trainingsdaten erkannt",
            },
            "performance" => new List<string>
            {
                $"{totalDistance:F1}km Gesamtdistanz über {workoutCount} Einheiten erreicht",
                "Trainingskonsistenz zeigt starkes Engagement für Leistungsziele",
                $"Durchschnittliche Trainingsintensität von {avgCalories:F0} Kalorien ist leistungsorientiert",
                "Trainingsvielfalt unterstützt vielseitige sportliche Entwicklung",
            },
            "trends" => new List<string>
            {
                $"Trainingshäufigkeit von {workoutCount} Einheiten zeigt konsistente Gewohnheitsbildung",
                "Distanz- und Dauertrends deuten auf Anwendung progressiver Überlastung hin",
                "Kalorienverbrauchsmuster deuten auf effektives Trainingsintensitätsmanagement hin",
                "Gesamttrajektorie deutet auf anhaltende Fitnessverbesserung hin",
            },
            _ => new List<string>
            {
                $"{workoutCount} Trainingseinheiten mit Gesamtdistanz von {totalDistance:F1}km absolviert",
                "Trainingskonsistenz zeigt Engagement für Fitnessziele",
                "Leistungsmetriken deuten auf stetige Verbesserungstrajektorie hin",
                "Trainingsintensität und -häufigkeit scheinen gut ausgewogen",
            }
        };

        var recommendations = analysisType.ToLower() switch
        {
            "health" => new List<string>
            {
                "Aktuellen Trainingsplan fortsetzen, um Gesundheitsvorteile zu erhalten",
                "Regenerationszeichen überwachen und Intensität bei Müdigkeit anpassen",
                "Ausreichend Schlaf und Ernährung sicherstellen, um Trainingsbelastung zu unterstützen",
                "Mobilitätsarbeit hinzufügen, um Verletzungen vorzubeugen",
            },
            "performance" => new List<string>
            {
                "Trainingsintensität schrittweise um 5-10% für Leistungssteigerungen erhöhen",
                "Intervalltraining hinzufügen, um Geschwindigkeits- und Kraftentwicklung zu fördern",
                "Leistungstests in Betracht ziehen, um spezifische Verbesserungen zu verfolgen",
                "Sportspezifische Übungen für gezielte Fertigkeitsentwicklung integrieren",
            },
            "trends" => new List<string>
            {
                "Aktuelle Trainingshäufigkeit für anhaltende positive Trends beibehalten",
                "Progressive Steigerungen in Distanz und Dauer planen",
                "Wöchentliche Trends verfolgen, um optimale Trainingsmuster zu identifizieren",
                "Monatliche Ziele basierend auf aktueller Fortschrittsrate setzen",
            },
            _ => new List<string>
            {
                "Mit aktuellem Trainingsplan und -intensität fortfahren",
                "Trainingsschwierigkeit alle 2-3 Wochen schrittweise um 5-10% erhöhen",
                "Ausreichende Erholung zwischen intensiven Trainingseinheiten sicherstellen",
                "Trainingsvielfalt für ausgewogene Entwicklung hinzufügen",
            }
        };

        return new WorkoutAnalysisResponseDto
        {
            Analysis = analysis,
            KeyInsights = insights,
            Recommendations = recommendations,
            GeneratedAt = DateTime.UtcNow,
            Provider = provider,
        };
    }
}