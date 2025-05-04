import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Athlete } from '../../../models/athlete.model';
import { AthleteService } from '../../../services/athlete.service';

@Component({
  selector: 'app-athlete-detail',
  templateUrl: './athlete-detail.component.html',
  standalone: true,
  imports: [CommonModule],
})
export class AthleteDetailComponent implements OnInit {
  athlete: Athlete | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private athleteService: AthleteService
  ) {}

  ngOnInit(): void {
    this.loadAthlete();
  }

  loadAthlete(): void {
    this.loading = true;
    const idParam = this.route.snapshot.paramMap.get('id');

    if (!idParam) {
      this.error = 'Ungültige Athleten-ID';
      this.loading = false;
      return;
    }

    const id = parseInt(idParam, 10);

    this.athleteService.getAthleteById(id).subscribe({
      next: (data) => {
        this.athlete = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Fehler beim Laden des Athleten: ' + err.message;
        this.loading = false;
      },
    });
  }

  editAthlete(): void {
    if (this.athlete) {
      this.router.navigate(['/athletes/edit', this.athlete.id]);
    }
  }

  deleteAthlete(): void {
    if (!this.athlete) return;

    if (confirm('Bist du sicher, dass du diesen Athleten löschen möchtest?')) {
      this.athleteService.deleteAthlete(this.athlete.id).subscribe({
        next: () => {
          this.router.navigate(['/athletes']);
        },
        error: (err) => {
          this.error = 'Fehler beim Löschen: ' + err.message;
        },
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/athletes']);
  }
}
