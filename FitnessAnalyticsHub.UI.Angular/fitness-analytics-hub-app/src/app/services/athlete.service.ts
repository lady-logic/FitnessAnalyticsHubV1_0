import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  Athlete,
  CreateAthleteDto,
  UpdateAthleteDto,
} from '../models/athlete.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AthleteService {
  private apiUrl = `${environment.apiUrl}/api/athlete`;

  constructor(private http: HttpClient) {}

  // Alle Athleten abrufen
  getAllAthletes(): Observable<Athlete[]> {
    return this.http
      .get<Athlete[]>(this.apiUrl)
      .pipe(catchError(this.handleError));
  }

  // Einen Athleten nach ID abrufen
  getAthleteById(id: number): Observable<Athlete> {
    return this.http
      .get<Athlete>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  // Einen neuen Athleten erstellen
  createAthlete(athlete: CreateAthleteDto): Observable<Athlete> {
    return this.http
      .post<Athlete>(this.apiUrl, athlete)
      .pipe(catchError(this.handleError));
  }

  // Einen Athleten aktualisieren
  updateAthlete(athlete: UpdateAthleteDto): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${athlete.id}`, athlete)
      .pipe(catchError(this.handleError));
  }

  // Einen Athleten löschen
  deleteAthlete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  // Athleten von Strava importieren
  importFromStrava(accessToken: string): Observable<Athlete> {
    return this.http
      .post<Athlete>(`${this.apiUrl}/import-from-strava`, { accessToken })
      .pipe(catchError(this.handleError));
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
        errorMessage += `\nDetails: ${error.error}`;
      }
    }

    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
