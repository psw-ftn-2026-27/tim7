import { Component, output } from '@angular/core';

// The card the app shell shows while the user has not decided about analytics. It injects
// nothing: it reports the choice through its outputs, and the shell tells the service.
@Component({
  selector: 'app-consent-banner',
  styleUrl: './consent-banner.scss',
  templateUrl: './consent-banner.html',
})
export class ConsentBanner {
  readonly accepted = output<void>();
  readonly declined = output<void>();
}
