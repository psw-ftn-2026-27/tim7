import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Home } from './core/home/home';

@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);

  private readonly homeIsShown = signal(false);

  protected readonly showHeader = computed(() => this.auth.isLoggedIn() || !this.homeIsShown());

  protected onPageShown(page: object): void {
    this.homeIsShown.set(page instanceof Home);
  }
}
