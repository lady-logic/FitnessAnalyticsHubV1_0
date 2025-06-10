import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, delay } from 'rxjs/operators';
import { AIAnalysis, WorkoutData } from '../models/activity.model';
import { environment } from '../../environments/environment';

export interface AnalysisRequest {
  recentWorkouts: WorkoutData[];
  analysisType: 'Performance' | 'Health' | 'Goals' | 'Trends';
  athleteId?: number;
  timeFrame?: 'week' | 'month' | 'quarter' | 'year';
}

@Injectable({
  providedIn: 'root',
})
export class AIService {
  private apiUrl = `${environment.apiUrl}/api/ai`;

  constructor(private http: HttpClient) {}

  // Workout-Analyse
  analyzeWorkout(request: AnalysisRequest): Observable<AIAnalysis> {
    // Falls der AI-Service noch nicht verfügbar ist, geben wir Mock-Daten zurück
    if (this.isAIServiceAvailable()) {
      return this.http
        .post<AIAnalysis>(`${this.apiUrl}/analyze-workout`, request)
        .pipe(catchError(this.handleError));
    } else {
      return this.getMockAnalysis(request);
    }
  }

  // Performance-Trends analysieren
  analyzePerformanceTrends(
    athleteId: number,
    timeFrame: string = 'month'
  ): Observable<AIAnalysis> {
    if (this.isAIServiceAvailable()) {
      return this.http
        .get<AIAnalysis>(
          `${this.apiUrl}/performance-trends/${athleteId}?timeFrame=${timeFrame}`
        )
        .pipe(catchError(this.handleError));
    } else {
      return this.getMockTrendsAnalysis();
    }
  }

  // Trainingsempfehlungen
  getTrainingRecommendations(athleteId: number): Observable<AIAnalysis> {
    if (this.isAIServiceAvailable()) {
      return this.http
        .get<AIAnalysis>(`${this.apiUrl}/recommendations/${athleteId}`)
        .pipe(catchError(this.handleError));
    } else {
      return this.getMockRecommendations();
    }
  }

  // Gesundheitsanalyse
  analyzeHealthMetrics(
    athleteId: number,
    workouts: WorkoutData[]
  ): Observable<AIAnalysis> {
    const request = {
      recentWorkouts: workouts,
      analysisType: 'Health' as const,
      athleteId,
    };

    if (this.isAIServiceAvailable()) {
      return this.http
        .post<AIAnalysis>(`${this.apiUrl}/health-analysis`, request)
        .pipe(catchError(this.handleError));
    } else {
      return this.getMockHealthAnalysis();
    }
  }

  // Prüft ob AI-Service verfügbar ist (kann später durch echten Health-Check ersetzt werden)
  private isAIServiceAvailable(): boolean {
    // Hier könnte man einen echten Health-Check implementieren
    return false; // Erstmal auf false setzen, bis AI-Backend verfügbar ist
  }

  // Mock-Daten für Entwicklung (werden später entfernt)
  private getMockAnalysis(request: AnalysisRequest): Observable<AIAnalysis> {
    const mockAnalysis: AIAnalysis = {
      analysis: `Basierend auf deinen letzten ${request.recentWorkouts.length} Aktivitäten zeigt sich eine positive Entwicklung. Deine durchschnittliche Trainingsintensität liegt im optimalen Bereich für kontinuierliche Verbesserung.`,
      keyInsights: [
        'Deine Ausdauerleistung hat sich in den letzten 4 Wochen um 12% verbessert',
        'Durchschnittliche Herzfrequenz liegt im Zielbereich für Fettverbrennung',
        'Regelmäßigkeit des Trainings ist sehr gut - 4.2 Einheiten pro Woche',
        'Empfehlung: Integriere mehr Intervalltraining für weitere Verbesserungen',
      ],
      recommendations: [
        'Füge 1-2 HIIT-Sessions pro Woche hinzu',
        'Erhöhe langsam die Laufstrecke um 10% pro Woche',
        'Plane Regenerationstage nach intensiven Trainingseinheiten',
      ],
      performanceScore: 78,
      trends: {
        direction: 'up',
        description: 'Steigende Leistungstendenz in den letzten 30 Tagen',
      },
    };

    return of(mockAnalysis).pipe(delay(1500)); // Simuliere API-Latenz
  }

  private getMockTrendsAnalysis(): Observable<AIAnalysis> {
    const mockTrends: AIAnalysis = {
      analysis:
        'Deine Performance-Trends zeigen eine konsistente Verbesserung über die letzten 3 Monate. Besonders bemerkenswert ist die Steigerung deiner Ausdauerkapazität.',
      keyInsights: [
        'Durchschnittsgeschwindigkeit um 8% gestiegen',
        'Herzfrequenz-Effizienz verbessert sich kontinuierlich',
        'Weniger Ermüdung bei gleicher Trainingsintensität',
        'Stärkste Verbesserung bei Läufen über 10km',
      ],
      performanceScore: 82,
      trends: {
        direction: 'up',
        description: 'Kontinuierliche Verbesserung seit 3 Monaten',
      },
    };

    return of(mockTrends).pipe(delay(1200));
  }

  private getMockRecommendations(): Observable<AIAnalysis> {
    const mockRecommendations: AIAnalysis = {
      analysis:
        'Basierend auf deinem aktuellen Trainingsniveau und deinen Zielen, hier sind personalisierte Empfehlungen für optimale Resultate.',
      keyInsights: [
        'Dein aktuelles Trainingsniveau: Fortgeschritten',
        'Hauptziel identifiziert: Ausdauerverbesserung',
        'Schwachstelle: Kraftausdauer könnte verbessert werden',
        'Regeneration: Gut eingeplant',
      ],
      recommendations: [
        'Integriere 2x wöchentlich Krafttraining',
        'Verwende die 80/20-Regel: 80% lockeres, 20% intensives Training',
        'Plane alle 4-6 Wochen eine Regenerationswoche',
        'Experimentiere mit Bergläufen für Kraftausdauer',
      ],
      performanceScore: 75,
    };

    return of(mockRecommendations).pipe(delay(1000));
  }

  private getMockHealthAnalysis(): Observable<AIAnalysis> {
    const mockHealth: AIAnalysis = {
      analysis:
        'Deine Gesundheitsmetriken zeigen ein ausgewogenes Trainingsprogramm. Die Herzfrequenz-Variabilität und Belastungsverteilung sind optimal.',
      keyInsights: [
        'Ruhepuls: Sehr gut (52 bpm)',
        'Herzfrequenz-Variabilität: Ausgezeichnet',
        'Übertraining-Risiko: Niedrig',
        'Schlafqualität beeinflusst Performance positiv',
      ],
      recommendations: [
        'Weiterhin auf ausreichend Schlaf achten (7-9h)',
        'Hydratation vor und nach Training optimieren',
        'Dehnung und Mobilität 3x pro Woche',
        'Stressmanagement durch Meditation oder Yoga',
      ],
      performanceScore: 88,
      trends: {
        direction: 'stable',
        description: 'Stabile und gesunde Trainingsbelastung',
      },
    };

    return of(mockHealth).pipe(delay(1300));
  }

  // Fehlerbehandlung
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'AI-Service temporär nicht verfügbar.';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Fehler: ${error.error.message}`;
    } else {
      errorMessage = `AI-Service Fehler (${error.status}): ${error.message}`;
    }

    console.error('AIService Fehler:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
