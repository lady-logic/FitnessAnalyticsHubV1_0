// src/main.ts

import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app/app.routes';
import { environment } from './environments/environment';

// Import the component directly
import { AppComponent } from './app/app.component';

// Produktionsoptimierungen
if (environment.production) {
  // Hier können Produktionsoptimierungen aktiviert werden
}

bootstrapApplication(AppComponent, {
  providers: [provideRouter(routes), provideHttpClient()],
}).catch((err) => console.error(err));
