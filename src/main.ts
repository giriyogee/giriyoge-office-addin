import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

function bootstrap(): void {
  bootstrapApplication(AppComponent, appConfig).catch((error: unknown) => {
    console.error('Angular bootstrap failed.', error);
  });
}

if (typeof Office !== 'undefined' && Office.onReady) {
  Office.onReady()
    .then(() => bootstrap())
    .catch((error: unknown) => {
      console.error('Office.js initialization failed.', error);
      bootstrap();
    });
} else {
  bootstrap();
}
