import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Athlete, UpdateAthleteDto } from '../../../models/athlete.model';
import { AthleteService } from '../../../services/athlete.service';

@Component({
  selector: 'app-athlete-edit',
  templateUrl: './athlete-edit.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class AthleteEditComponent implements OnInit {
  athleteForm: FormGroup;
  athlete: Athlete | null = null;
  athleteId: number = 0;
  loading = false;
  submitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private athleteService: AthleteService
  ) {
    this.athleteForm = this.fb.group({
      id: [0, Validators.required],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      dateOfBirth: ['', Validators.required],
      weight: ['', [Validators.required, Validators.min(0)]],
      height: ['', [Validators.required, Validators.min(0)]],
      // Weitere Felder entsprechend deinem UpdateAthleteDto
    });
  }

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

    this.athleteId = parseInt(idParam, 10);

    this.athleteService.getAthleteById(this.athleteId).subscribe({
      next: (data) => {
        this.athlete = data;
        this.populateForm(data);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Fehler beim Laden des Athleten: ' + err.message;
        this.loading = false;
      },
    });
  }

  populateForm(athlete: Athlete): void {
    // Sichere Konvertierung des Datums
    let dateOfBirth = '';
    try {
      const date = new Date(athlete.dateOfBirth);
      if (!isNaN(date.getTime())) {
        dateOfBirth = date.toISOString().split('T')[0];
      }
    } catch (error) {
      console.error('Fehler bei der Datumskonvertierung:', error);
    }

    this.athleteForm.patchValue({
      id: athlete.id,
      name: athlete.name,
      email: athlete.email,
      dateOfBirth: dateOfBirth,
      weight: athlete.weight,
      height: athlete.height
    });
  }

  onSubmit(): void {
    if (this.athleteForm.invalid || this.submitting) {
      this.markFormGroupTouched(this.athleteForm);
      return;
    }

    this.submitting = true;
    const formValue = this.athleteForm.value;
    
    // Konvertiere das Datum-String in ein Date-Objekt
    const updatedAthlete: UpdateAthleteDto = {
      ...formValue,
      dateOfBirth: formValue.dateOfBirth ? new Date(formValue.dateOfBirth) : new Date()
    };

    this.athleteService.updateAthlete(updatedAthlete).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/athletes', this.athleteId]);
      },
      error: (err) => {
        this.error = 'Fehler beim Aktualisieren des Athleten: ' + err.message;
        this.submitting = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/athletes', this.athleteId]);
  }

  // Hilfsmethode, um alle Formularfelder als "berührt" zu markieren,
  // damit Validierungsfehler angezeigt werden
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if ((control as any).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }
}
