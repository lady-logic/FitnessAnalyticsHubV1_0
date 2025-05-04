import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CreateAthleteDto } from '../../../models/athlete.model';
import { AthleteService } from '../../../services/athlete.service';

@Component({
  selector: 'app-athlete-create',
  templateUrl: './athlete-create.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class AthleteCreateComponent implements OnInit {
  athleteForm: FormGroup;
  submitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private athleteService: AthleteService,
    private router: Router
  ) {
    this.athleteForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      dateOfBirth: ['', Validators.required],
      weight: ['', [Validators.required, Validators.min(0)]],
      height: ['', [Validators.required, Validators.min(0)]],
      // Weitere Felder entsprechend deinem CreateAthleteDto
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.athleteForm.invalid || this.submitting) {
      // Formular markieren, damit Validierungsfehler angezeigt werden
      this.markFormGroupTouched(this.athleteForm);
      return;
    }

    this.submitting = true;
    const newAthlete: CreateAthleteDto = this.athleteForm.value;

    this.athleteService.createAthlete(newAthlete).subscribe({
      next: (createdAthlete) => {
        this.submitting = false;
        this.router.navigate(['/athletes', createdAthlete.id]);
      },
      error: (err) => {
        this.error = 'Fehler beim Erstellen des Athleten: ' + err.message;
        this.submitting = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/athletes']);
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
