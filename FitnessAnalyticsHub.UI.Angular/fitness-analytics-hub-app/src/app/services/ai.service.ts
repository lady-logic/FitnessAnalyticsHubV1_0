import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, delay, map } from 'rxjs/operators';
import { AIAnalysis, WorkoutData } from '../models/activity.model';
import { environment } from '../../environments/environment';

export interface AnalysisRequest {
  recentWorkouts: WorkoutData[];
  analysisType: 'Performance' | 'Health' | 'Goals' | 'Trends';
  athleteId?: number;
  timeFrame?: 'week' | 'month' | 'quarter' | 'year';
}

// DTOs für AI-Assistant API
interface AIMotivationRequest {
  athleteProfile: {
    name: string;
    fitnessLevel: string;
    primaryGoal: string;
  };
  recentWorkouts: Array<{
    date: string;
    activityType: string;
    distance: number;
    duration: number;
    calories: number;
  }>;
  preferredTone?: string;
  contextualInfo?: string;
}

interface AIWorkoutAnalysisRequest {
  athleteProfile?: {
    name: string;
    fitnessLevel: string;
    primaryGoal: string;
  };
  recentWorkouts: Array<{
    date: string;
    activityType: string;
    distance: number;
    duration: number;
    calories: number;
  }>;
  analysisType: string;
  focusAreas?: string[];
}

interface AIResponse {
  analysis?: string; // ← Klein wie in der Response
  keyInsights?: string[]; // ← Klein wie in der Response
  recommendations?: string[]; // ← Klein wie in der Response
  generatedAt: string; // ← Klein wie in der Response
  source: string; // ← Klein wie in der Response

  // Fallback für andere Endpoints
  motivationalMessage?: string;
  actionableTips?: string[];
  quote?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AIService {
  private apiUrl = `${environment.aiApiUrl}/api/AI`; // ← Verwendet aiApiUrl!

  constructor(private http: HttpClient) {}

  // Workout-Analyse mit echten AI-Calls
  analyzeWorkout(request: AnalysisRequest): Observable<AIAnalysis> {
    const aiRequest: AIWorkoutAnalysisRequest = {
      athleteProfile: {
        name: 'Fitness Enthusiast',
        fitnessLevel: 'Intermediate',
        primaryGoal: 'Performance Improvement',
      },
      recentWorkouts: request.recentWorkouts.map((workout) => ({
        date: workout.date,
        activityType: workout.activityType,
        distance: workout.distance,
        duration: workout.duration || 0,
        calories: workout.calories || 0,
      })),
      analysisType: request.analysisType || 'Performance',
      focusAreas: ['endurance', 'consistency'],
    };

    return this.http
      .post<AIResponse>(`${this.apiUrl}/analysis`, aiRequest)
      .pipe(
        map((response) => this.mapToAIAnalysis(response)),
        catchError((err) => {
          console.warn('AI-Analyse fehlgeschlagen, verwende Mock-Daten:', err);
          return this.getMockAnalysis(request);
        })
      );
  }

  // Performance-Trends analysieren
  analyzePerformanceTrends(
    athleteId: number,
    timeFrame: string = 'month'
  ): Observable<AIAnalysis> {
    // Erstelle Request basierend auf Demo-Daten für Trends
    const trendRequest: AnalysisRequest = {
      recentWorkouts: this.getDemoWorkouts(),
      analysisType: 'Trends',
      athleteId,
      timeFrame: timeFrame as any,
    };

    return this.analyzeWorkout(trendRequest);
  }

  // Trainingsempfehlungen
  getTrainingRecommendations(athleteId: number): Observable<AIAnalysis> {
    const motivationRequest: AIMotivationRequest = {
      athleteProfile: {
        name: 'Fitness Enthusiast',
        fitnessLevel: 'Intermediate',
        primaryGoal: 'Training Improvement',
      },
      recentWorkouts: this.getDemoWorkouts().map((w) => ({
        date: w.date,
        activityType: w.activityType,
        distance: w.distance,
        duration: w.duration || 0,
        calories: w.calories || 0,
      })),
      preferredTone: 'Motivational',
      contextualInfo: 'Looking for training recommendations',
    };

    return this.http
      .post<AIResponse>(`${this.apiUrl}/motivation`, motivationRequest)
      .pipe(
        map((response) => this.mapMotivationToAnalysis(response)),
        catchError((err) => {
          console.warn(
            'Training-Empfehlungen fehlgeschlagen, verwende Mock-Daten:',
            err
          );
          return this.getMockRecommendations();
        })
      );
  }

  // Gesundheitsanalyse
  analyzeHealthMetrics(
    athleteId: number,
    workouts: WorkoutData[]
  ): Observable<AIAnalysis> {
    const healthRequest: AnalysisRequest = {
      recentWorkouts: workouts,
      analysisType: 'Health',
      athleteId,
    };

    return this.analyzeWorkout(healthRequest);
  }

  // AI Health Check
  checkAIHealth(): Observable<boolean> {
    return this.http.get<any>(`${this.apiUrl}/health`).pipe(
      map((response) => response.isHealthy || false),
      catchError(() => of(false))
    );
  }

  // Mapping-Funktionen
  private mapToAIAnalysis(response: AIResponse): AIAnalysis {
    return {
      analysis: response.analysis || 'AI analysis completed successfully.',
      keyInsights: response.keyInsights || [
        'Your training shows consistent progress',
        'Performance metrics are improving',
        'Keep up the great work!',
      ],
      recommendations: response.recommendations || [
        'Continue with current training schedule',
        'Focus on gradual progression',
        'Ensure adequate recovery time',
      ],
      performanceScore: Math.floor(Math.random() * 20) + 70, // 70-90
      trends: {
        direction: 'up' as const,
        description: 'Positive trend detected',
      },
    };
  }

  private mapMotivationToAnalysis(response: AIResponse): AIAnalysis {
    return {
      analysis:
        response.motivationalMessage ||
        'Stay motivated and keep pushing your limits!',
      keyInsights: response.actionableTips || [
        'Consistency is key to success',
        'Small improvements compound over time',
        'Your dedication is paying off',
      ],
      recommendations: response.recommendations || [
        'Set small, achievable daily goals',
        'Track your progress regularly',
        'Celebrate every milestone',
      ],
      performanceScore: Math.floor(Math.random() * 15) + 80, // 80-95 für Motivation
    };
  }

  // Demo-Daten für AI-Requests
  private getDemoWorkouts(): WorkoutData[] {
    return [
      {
        date: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(),
        activityType: 'Run',
        distance: 5.2,
        duration: 1680,
        calories: 420,
      },
      {
        date: new Date(Date.now() - 1000 * 60 * 60 * 48).toISOString(),
        activityType: 'Ride',
        distance: 24.8,
        duration: 4500,
        calories: 890,
      },
      {
        date: new Date(Date.now() - 1000 * 60 * 60 * 72).toISOString(),
        activityType: 'Run',
        distance: 3.1,
        duration: 1080,
        calories: 245,
      },
    ];
  }

  // Mock-Daten als Fallback (unverändert)
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

    return of(mockAnalysis).pipe(delay(1500));
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
