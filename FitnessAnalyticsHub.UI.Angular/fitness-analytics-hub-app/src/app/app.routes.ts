// src/app/app.routes.ts

import { Routes } from '@angular/router';
import { AthleteListComponent } from './components/athletes/athlete-list/athlete-list.component';
import { AthleteDetailComponent } from './components/athletes/athlete-detail/athlete-detail.component';
import { AthleteCreateComponent } from './components/athletes/athlete-create/athlete-create.component';
import { AthleteEditComponent } from './components/athletes/athlete-edit/athlete-edit.component';
import { StravaImportComponent } from './components/athletes/strava-import/strava-import.component';

export const routes: Routes = [
  { path: 'athletes', component: AthleteListComponent },
  { path: 'athletes/create', component: AthleteCreateComponent },
  { path: 'athletes/edit/:id', component: AthleteEditComponent },
  { path: 'athletes/strava-import', component: StravaImportComponent },
  { path: 'athletes/:id', component: AthleteDetailComponent },
  { path: '', redirectTo: 'athletes', pathMatch: 'full' },
  { path: '**', redirectTo: 'athletes' },
];
