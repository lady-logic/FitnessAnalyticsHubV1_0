import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Athlete } from '../../../models/athlete.model';
import { AthleteService } from '../../../services/athlete.service';

@Component({
  selector: 'app-athlete-list',
  templateUrl: './athlete-list.component.html',
  standalone: true,
  imports: [CommonModule],
})
export class AthleteListComponent implements OnInit {
  athletes: Athlete[] = [];
  loading = false;
  error: string | null = null;

  constructor(private athleteService: AthleteService, private router: Router) {}

  ngOnInit(): void {
    this.loadAthletes();
  }

  loadAthletes(): void {
    this.loading = true;
    this.error = null;

    this.athleteService.getAllAthletes().subscribe({
      next: (data) => {
        this.athletes = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Fehler beim Laden der Athleten: ' + err.message;
        this.loading = false;
      },
    });
  }

  viewDetails(id: number): void {
    this.router.navigate(['/athletes', id]);
  }

  editAthlete(id: number): void {
    this.router.navigate(['/athletes/edit', id]);
  }

  deleteAthlete(id: number): void {
    if (confirm('Bist du sicher, dass du diesen Athleten löschen möchtest?')) {
      this.athleteService.deleteAthlete(id).subscribe({
        next: () => {
          this.loadAthletes(); // Liste neu laden
        },
        error: (err) => {
          this.error = 'Fehler beim Löschen: ' + err.message;
        },
      });
    }
  }

  createNewAthlete(): void {
    this.router.navigate(['/athletes/create']);
  }

  importFromStrava(): void {
    this.router.navigate(['/athletes/strava-import']);
  }
}
