import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AthleteService } from '../../../services/athlete.service';

@Component({
  selector: 'app-strava-import',
  templateUrl: './strava-import.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class StravaImportComponent implements OnInit {
  importForm: FormGroup;
  submitting = false;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private fb: FormBuilder,
    private athleteService: AthleteService,
    private router: Router
  ) {
    this.importForm = this.fb.group({
      accessToken: ['', Validators.required],
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.importForm.invalid || this.submitting) {
      return;
    }

    this.submitting = true;
    this.error = null;
    this.success = null;

    const accessToken = this.importForm.value.accessToken;

    this.athleteService.importFromStrava(accessToken).subscribe({
      next: (athlete) => {
        this.submitting = false;
        this.success = `Athlet ${athlete.firstName} ${athlete.lastName} wurde erfolgreich importiert.`;
        setTimeout(() => {
          this.router.navigate(['/athletes', athlete.id]);
        }, 2000);
      },
      error: (err) => {
        this.error = 'Fehler beim Import von Strava: ' + err.message;
        this.submitting = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/athletes']);
  }
}
