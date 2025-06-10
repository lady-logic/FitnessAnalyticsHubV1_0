import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ActivityDto, ActivityStatistics } from '../models/activity.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class FitnessService {
  private apiUrl = `${environment.apiUrl}/api/activity`;

  constructor(private http: HttpClient) {}

  // Athleten-Statistiken abrufen
  getAthleteStatistics(athleteId: number): Observable<ActivityStatistics> {
    return this.http
      .get<ActivityStatistics>(`${this.apiUrl}/statistics/${athleteId}`)
      .pipe(catchError(this.handleError));
  }

  // Alle Aktivitäten eines Athleten abrufen
  getAthleteActivities(athleteId: number): Observable<ActivityDto[]> {
    return this.http
      .get<ActivityDto[]>(`${this.apiUrl}/athlete/${athleteId}`)
      .pipe(catchError(this.handleError));
  }

  // Neueste Aktivitäten eines Athleten abrufen
  getRecentActivities(
    athleteId: number,
    count: number = 10
  ): Observable<ActivityDto[]> {
    return this.getAthleteActivities(athleteId).pipe(
      map((activities) =>
        activities
          .sort(
            (a, b) =>
              new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
          )
          .slice(0, count)
      )
    );
  }

  // Aktivität nach ID abrufen
  getActivityById(activityId: number): Observable<ActivityDto> {
    return this.http
      .get<ActivityDto>(`${this.apiUrl}/${activityId}`)
      .pipe(catchError(this.handleError));
  }

  // Aktivitäten nach Zeitraum filtern
  getActivitiesByDateRange(
    athleteId: number,
    startDate: Date,
    endDate: Date
  ): Observable<ActivityDto[]> {
    const start = startDate.toISOString().split('T')[0];
    const end = endDate.toISOString().split('T')[0];

    return this.http
      .get<ActivityDto[]>(
        `${this.apiUrl}/athlete/${athleteId}/range?start=${start}&end=${end}`
      )
      .pipe(catchError(this.handleError));
  }

  // Aktivitäten nach Sportart filtern
  getActivitiesBySport(
    athleteId: number,
    sportType: string
  ): Observable<ActivityDto[]> {
    return this.getAthleteActivities(athleteId).pipe(
      map((activities) =>
        activities.filter(
          (activity) =>
            activity.sportType.toLowerCase() === sportType.toLowerCase()
        )
      )
    );
  }

  // Fehlerbehandlung
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Ein unbekannter Fehler ist aufgetreten.';

    if (error.error instanceof ErrorEvent) {
      // Client-seitiger Fehler
      errorMessage = `Fehler: ${error.error.message}`;
    } else {
      // Server-seitiger Fehler
      errorMessage = `Status: ${error.status}\nMeldung: ${error.message}`;
      if (error.error) {
        errorMessage += `\nDetails: ${JSON.stringify(error.error)}`;
      }
    }

    console.error('FitnessService Fehler:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
