import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { ConsentBanner } from './core/consent-banner/consent-banner';
import { Home } from './core/home/home';
import { Analytics } from './shared/analytics/analytics';

@Component({
  imports: [ConsentBanner, RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);

  // Injected here so that the service exists from the start and hears the first navigation.
  protected readonly analytics = inject(Analytics);

  private readonly homeIsShown = signal(false);

  protected readonly showHeader = computed(() => this.auth.isLoggedIn() || !this.homeIsShown());

  protected onPageShown(page: object): void {
    this.homeIsShown.set(page instanceof Home);
  }
}
