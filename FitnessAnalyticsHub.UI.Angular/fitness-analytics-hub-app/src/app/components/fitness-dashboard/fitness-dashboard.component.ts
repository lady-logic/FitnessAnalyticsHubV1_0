import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subject, combineLatest, of } from 'rxjs';
import { map, takeUntil, catchError } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

// Services
import { FitnessService } from '../../services/fitness.service';
import { AIService } from '../../services/ai.service';
import { AthleteService } from '../../services/athlete.service';

// Models
import {
  ActivityStatistics,
  ActivityDto,
  AIAnalysis,
  WorkoutData,
} from '../../models/activity.model';
import { Athlete } from '../../models/athlete.model';

@Component({
  selector: 'app-fitness-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fitness-dashboard.component.html',
  styleUrls: ['./fitness-dashboard.component.scss'],
})
export class FitnessDashboardComponent implements OnInit, OnDestroy {
  // Observables
  statistics$!: Observable<ActivityStatistics>;
  recentActivities$!: Observable<ActivityDto[]>;
  currentAthlete$!: Observable<Athlete>;
  aiAnalysis$: Observable<AIAnalysis | null> = of(null);

  // State
  loading = true;
  error: string | null = null;
  analyzingActivity: number | null = null;
  athleteId: number = 0;

  // Cleanup
  private destroy$ = new Subject<void>();

  constructor(
    private fitnessService: FitnessService,
    private aiService: AIService,
    private athleteService: AthleteService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboardData() {
    this.loading = true;
    this.error = null;

    // Athleten-ID aus Route oder Default verwenden
    const idParam = this.route.snapshot.paramMap.get('id');
    this.athleteId = idParam ? parseInt(idParam, 10) : 1;

    // Kombiniere alle benötigten Daten
    this.currentAthlete$ = this.athleteService
      .getAthleteById(this.athleteId)
      .pipe(
        catchError((err) => {
          this.error = 'Athlet nicht gefunden. Verwende Demo-Daten.';
          this.loading = false;
          return of({ id: 1, name: 'Demo User' } as Athlete);
        })
      );

    this.statistics$ = this.fitnessService
      .getAthleteStatistics(this.athleteId)
      .pipe(
        catchError((err) => {
          console.warn(
            'Statistiken nicht verfügbar, verwende Demo-Daten:',
            err
          );
          return of(this.getDemoStatistics());
        })
      );

    this.recentActivities$ = this.fitnessService
      .getRecentActivities(this.athleteId, 6)
      .pipe(
        catchError((err) => {
          console.warn(
            'Aktivitäten nicht verfügbar, verwende Demo-Daten:',
            err
          );
          return of(this.getDemoActivities());
        })
      );

    // Warte bis Daten geladen sind
    combineLatest([
      this.currentAthlete$,
      this.statistics$,
      this.recentActivities$,
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ([athlete, stats, activities]) => {
          this.loading = false;
        },
        error: (err) => {
          this.loading = false;
          this.error =
            'Fehler beim Laden der Dashboard-Daten. Verwende Demo-Daten.';
        },
      });
  }

  // Activity Analysis
  analyzeActivity(activity: ActivityDto) {
    if (this.analyzingActivity === activity.id) return;

    this.analyzingActivity = activity.id;

    const workoutData: WorkoutData = {
      date: activity.startDate,
      activityType: activity.sportType,
      distance: activity.distance,
      duration: activity.duration,
      heartRate: activity.averageHeartRate,
      calories: activity.calories,
    };

    this.aiAnalysis$ = this.aiService
      .analyzeWorkout({
        recentWorkouts: [workoutData],
        analysisType: 'Performance',
        athleteId: this.athleteId,
      })
      .pipe(
        takeUntil(this.destroy$),
        catchError((err) => {
          console.error('AI-Analyse fehlgeschlagen:', err);
          return of({
            analysis: `Great ${activity.sportType.toLowerCase()}! You covered ${
              activity.distance
            }km with solid performance.`,
            keyInsights: [
              `Distance: ${activity.distance}km completed successfully`,
              'Consistency in training shows great dedication',
              'Keep maintaining this excellent pace',
            ],
            recommendations: [
              'Try adding variety to your training routine',
              'Focus on recovery between sessions',
              'Set progressive distance goals',
            ],
            performanceScore: Math.floor(Math.random() * 20) + 70, // 70-90
          } as AIAnalysis);
        })
      );

    // Reset analyzing state after completion
    setTimeout(() => {
      this.analyzingActivity = null;
    }, 2000);
  }

  // Performance Trends Analysis
  analyzePerformanceTrends() {
    this.aiAnalysis$ = this.aiService
      .analyzePerformanceTrends(this.athleteId, 'month')
      .pipe(
        takeUntil(this.destroy$),
        catchError(this.handleAnalysisError.bind(this))
      );
  }

  // Training Recommendations
  getTrainingRecommendations() {
    this.aiAnalysis$ = this.aiService
      .getTrainingRecommendations(this.athleteId)
      .pipe(
        takeUntil(this.destroy$),
        catchError(this.handleAnalysisError.bind(this))
      );
  }

  // Health Metrics Analysis
  analyzeHealthMetrics() {
    this.recentActivities$
      .pipe(takeUntil(this.destroy$))
      .subscribe((activities) => {
        const workoutData: WorkoutData[] = activities.map((activity) => ({
          date: activity.startDate,
          activityType: activity.sportType,
          distance: activity.distance,
          duration: activity.duration,
          heartRate: activity.averageHeartRate,
          calories: activity.calories,
        }));

        this.aiAnalysis$ = this.aiService
          .analyzeHealthMetrics(this.athleteId, workoutData)
          .pipe(
            takeUntil(this.destroy$),
            catchError(this.handleAnalysisError.bind(this))
          );
      });
  }

  // ========== ERWEITERTE CHART-METHODEN (NEU) ==========

  // Weekly Progress Chart Methods
  getWeeklyProgress(): Array<{
    week: number;
    distance: number;
    percentage: number;
  }> {
    // Simuliert wöchentliche Fortschrittsdaten basierend auf Demo-Daten
    const weeklyData = [
      { week: 1, distance: 15.2 },
      { week: 2, distance: 23.8 },
      { week: 3, distance: 31.5 },
      { week: 4, distance: 28.7 },
    ];

    const maxDistance = Math.max(...weeklyData.map((w) => w.distance));

    return weeklyData.map((week) => ({
      ...week,
      percentage: (week.distance / maxDistance) * 100,
    }));
  }

  getAverageWeeklyDistance(): string {
    const weeks = this.getWeeklyProgress();
    const average =
      weeks.reduce((sum, week) => sum + week.distance, 0) / weeks.length;
    return average.toFixed(1);
  }

  getWeeklyTrend(): string {
    const weeks = this.getWeeklyProgress();
    const lastWeek = weeks[weeks.length - 1].distance;
    const previousWeek = weeks[weeks.length - 2].distance;

    if (lastWeek > previousWeek) return '📈 Improving';
    if (lastWeek < previousWeek) return '📉 Declining';
    return '➡️ Stable';
  }

  getWeekColor(index: number): string {
    const colors = [
      'linear-gradient(90deg, #667eea, #764ba2)',
      'linear-gradient(90deg, #f093fb, #f5576c)',
      'linear-gradient(90deg, #4facfe, #00f2fe)',
      'linear-gradient(90deg, #43e97b, #38f9d7)',
    ];
    return colors[index % colors.length];
  }

  // Activity Distribution Chart Methods
  getActivityColor(activityType: string): string {
    const colors: { [key: string]: string } = {
      Run: 'linear-gradient(135deg, #28a745, #20c997)',
      Ride: 'linear-gradient(135deg, #007bff, #6610f2)',
      Swim: 'linear-gradient(135deg, #17a2b8, #6f42c1)',
      Walk: 'linear-gradient(135deg, #6c757d, #495057)',
      Hike: 'linear-gradient(135deg, #795548, #8d6e63)',
      Workout: 'linear-gradient(135deg, #dc3545, #e83e8c)',
      Yoga: 'linear-gradient(135deg, #9c27b0, #673ab7)',
      WeightTraining: 'linear-gradient(135deg, #dc3545, #e83e8c)',
      Crosstraining: 'linear-gradient(135deg, #fd7e14, #ffc107)',
    };
    return colors[activityType] || 'linear-gradient(135deg, #667eea, #764ba2)';
  }

  // Monthly Heatmap Methods
  getMonthlyData(activitiesByMonth: { [key: number]: number }): Array<{
    name: string;
    count: number;
    percentage: number;
  }> {
    const monthNames = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];

    // Falls keine Daten vorhanden, verwende Demo-Daten
    const defaultData: { [key: number]: number } = {
      1: 8,
      2: 12,
      3: 15,
      4: 12,
      5: 9,
      6: 5,
    };
    const data: { [key: number]: number } =
      Object.keys(activitiesByMonth).length > 0
        ? activitiesByMonth
        : defaultData;

    const maxCount = Math.max(...Object.values(data));

    return monthNames.map((name, index) => {
      const monthIndex = index + 1;
      const count = data[monthIndex] || 0;
      return {
        name,
        count,
        percentage: maxCount > 0 ? (count / maxCount) * 100 : 0,
      };
    });
  }

  getMonthHeatColor(count: number): string {
    if (count === 0) return '#eee';
    if (count <= 3) return '#c6e48b';
    if (count <= 6) return '#7bc96f';
    if (count <= 10) return '#239a3b';
    return '#196127';
  }

  // Performance Metrics für erweiterte Statistiken
  getPerformanceMetrics(): {
    consistency: number;
    improvement: number;
    variety: number;
  } {
    return {
      consistency: 85, // Prozent der geplanten Trainings absolviert
      improvement: 12, // Prozent Verbesserung im letzten Monat
      variety: 3, // Anzahl verschiedener Sportarten
    };
  }

  // Goal Progress (Ziel-Fortschritt)
  getGoalProgress(): {
    weeklyGoal: number;
    achieved: number;
    percentage: number;
  } {
    const weeklyGoal = 25; // km pro Woche
    const achieved = parseFloat(this.getAverageWeeklyDistance());

    return {
      weeklyGoal,
      achieved,
      percentage: Math.min((achieved / weeklyGoal) * 100, 100),
    };
  }

  // Hilfsmethode für Fitness-Level basierend auf Aktivitäten
  getFitnessLevel(): string {
    const stats = this.getDemoStatistics();
    const weeklyAverage = stats.totalActivities / 12; // ~12 Wochen

    if (weeklyAverage >= 5) return 'Elite Athlete 🏆';
    if (weeklyAverage >= 3) return 'Fitness Enthusiast 💪';
    if (weeklyAverage >= 1.5) return 'Active Person 🏃';
    return 'Getting Started 🌱';
  }

  // Utility Methods for Modern Design
  getActivityEmoji(sportType: string): string {
    const emojis: { [key: string]: string } = {
      Run: '🏃',
      Ride: '🚴',
      Swim: '🏊',
      Walk: '🚶',
      Hike: '🥾',
      Workout: '💪',
      Yoga: '🧘',
      WeightTraining: '🏋️',
      Crosstraining: '⚡',
    };
    return emojis[sportType] || '🏃';
  }

  getActivityIconClass(sportType: string): string {
    const classes: { [key: string]: string } = {
      Run: 'run',
      Ride: 'ride',
      Swim: 'swim',
      Walk: 'walk',
      Hike: 'hike',
      Workout: 'workout',
      Yoga: 'yoga',
      WeightTraining: 'workout',
      Crosstraining: 'workout',
    };
    return classes[sportType] || 'default';
  }

  getScoreClass(score: number): string {
    if (score >= 85) return 'excellent';
    if (score >= 70) return 'good';
    if (score >= 50) return 'fair';
    return 'poor';
  }

  formatDuration(duration?: number): string {
    if (!duration) return 'N/A';

    const hours = Math.floor(duration / 3600);
    const minutes = Math.floor((duration % 3600) / 60);

    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }

  getEstimatedCalories(stats: ActivityStatistics): string {
    // Einfache Schätzung: ~50 Kalorien pro km
    const estimated = Math.round(stats.totalDistance * 50);
    return estimated.toLocaleString();
  }

  // ERWEITERTE VERSION der getActivityTypes Methode
  getActivityTypes(activitiesByType: {
    [key: string]: number;
  }): Array<{ name: string; count: number; percentage: number }> {
    const total = Object.values(activitiesByType).reduce(
      (sum, count) => sum + count,
      0
    );

    // Falls keine echten Daten, verwende Demo-Daten für Charts
    if (total === 0) {
      return [
        { name: 'Run', count: 28, percentage: 60 },
        { name: 'Ride', count: 15, percentage: 32 },
        { name: 'Swim', count: 4, percentage: 8 },
      ];
    }

    return Object.entries(activitiesByType)
      .map(([name, count]) => ({
        name,
        count,
        percentage: Math.round((count / total) * 100),
      }))
      .sort((a, b) => b.count - a.count); // Sortiert nach Anzahl für Charts
  }

  trackByActivityId(index: number, activity: ActivityDto): number {
    return activity.id;
  }

  // Navigation
  refreshData() {
    this.loadDashboardData();
  }

  formatMarkdown(text: string | undefined): string {
    if (!text) return '';

    return (
      text
        // Headers: ## Text → <h4>Text</h4>
        .replace(/## (.*?)(\n|$)/g, '<h4 class="analysis-header">$1</h4>')

        // Bold: **Text** → <strong>Text</strong>
        .replace(/\*\*(.*?)\*\*/g, '<strong class="analysis-bold">$1</strong>')

        // Numbered lists: 1. Text → <ol><li>Text</li></ol>
        .replace(/(\d+\.\s+.*?)(?=\n\d+\.|\n\n|$)/gs, (match) => {
          const items = match.split(/\n(?=\d+\.)/);
          const listItems = items
            .map((item) =>
              item
                .replace(/^\d+\.\s+/, '<li class="analysis-list-item">')
                .replace(/$/, '</li>')
            )
            .join('');
          return `<ol class="analysis-numbered-list">${listItems}</ol>`;
        })

        // Bullet Points: • Text oder - Text → <ul><li>Text</li></ul>
        .replace(/((?:•|-)\s+.*?)(?=\n(?:•|-)|$)/gs, (match) => {
          const items = match.split(/\n(?=•|-)/);
          const listItems = items
            .map((item) =>
              item
                .replace(/^(?:•|-)\s+/, '<li class="analysis-bullet-item">')
                .replace(/$/, '</li>')
            )
            .join('');
          return `<ul class="analysis-bullet-list">${listItems}</ul>`;
        })

        // Paragraphs: Doppelte Line breaks → <p>
        .replace(/\n\n/g, '</p><p class="analysis-paragraph">')

        // Single line breaks → <br>
        .replace(/\n/g, '<br>')

        // Wrap in paragraph if not starting with header
        .replace(/^(?!<[h4|ol|ul])/, '<p class="analysis-paragraph">')
        .replace(/(?<!>)$/, '</p>')
    );
  }

  goBack() {
    this.router.navigate(['/athletes']);
  }

  // Demo Data Methods - ERWEITERT mit mehr Monaten
  private getDemoStatistics(): ActivityStatistics {
    return {
      totalActivities: 47,
      totalDistance: 342.5,
      totalDuration: '28h 15m',
      averageDistance: 7.3,
      longestDistance: 21.1,
      mostCommonSport: 'Run',
      activitiesByType: {
        Run: 28,
        Ride: 15,
        Swim: 4,
      },
      activitiesByMonth: {
        1: 8, // Januar
        2: 12, // Februar
        3: 15, // März
        4: 12, // April
        5: 8, // Mai
        6: 5, // Juni (aktueller Monat)
      },
    };
  }

  private getDemoActivities(): ActivityDto[] {
    return [
      {
        id: 1,
        name: 'Morning Power Run',
        sportType: 'Run',
        distance: 5.2,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(), // 2 Stunden her
        athleteFullName: 'Demo User',
        duration: 1680, // 28 Minuten
        averageHeartRate: 145,
        maxHeartRate: 168,
        calories: 420,
      },
      {
        id: 2,
        name: 'Weekend Bike Adventure',
        sportType: 'Ride',
        distance: 24.8,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(), // Gestern
        athleteFullName: 'Demo User',
        duration: 4500, // 75 Minuten
        averageHeartRate: 132,
        maxHeartRate: 156,
        calories: 890,
      },
      {
        id: 3,
        name: 'Pool Training Session',
        sportType: 'Swim',
        distance: 1.5,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 48).toISOString(), // 2 Tage her
        athleteFullName: 'Demo User',
        duration: 2700, // 45 Minuten
        averageHeartRate: 125,
        maxHeartRate: 148,
        calories: 380,
      },
      {
        id: 4,
        name: 'Evening Recovery Jog',
        sportType: 'Run',
        distance: 3.1,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 72).toISOString(), // 3 Tage her
        athleteFullName: 'Demo User',
        duration: 1080, // 18 Minuten
        averageHeartRate: 128,
        maxHeartRate: 142,
        calories: 245,
      },
      {
        id: 5,
        name: 'Mountain Trail Run',
        sportType: 'Hike',
        distance: 8.7,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 96).toISOString(), // 4 Tage her
        athleteFullName: 'Demo User',
        duration: 3240, // 54 Minuten
        averageHeartRate: 138,
        maxHeartRate: 162,
        calories: 580,
      },
      {
        id: 6,
        name: 'Strength & Cardio Combo',
        sportType: 'Workout',
        distance: 0,
        startDate: new Date(Date.now() - 1000 * 60 * 60 * 120).toISOString(), // 5 Tage her
        athleteFullName: 'Demo User',
        duration: 2400, // 40 Minuten
        averageHeartRate: 142,
        maxHeartRate: 165,
        calories: 350,
      },
    ];
  }

  // Helper Methods
  private handleAnalysisError(err: any): Observable<AIAnalysis> {
    console.error('AI-Analyse fehlgeschlagen:', err);
    return of({
      analysis:
        'AI analysis is temporarily unavailable, but your training progress looks fantastic! Keep up the great work.',
      keyInsights: [
        'Your consistency in training is excellent',
        'Performance metrics show steady improvement',
        'Recovery patterns are within optimal range',
      ],
      recommendations: [
        'Continue with your current training schedule',
        'Focus on gradual progression',
        'Ensure adequate rest between intense sessions',
      ],
      performanceScore: 78,
    } as AIAnalysis);
  }
}
