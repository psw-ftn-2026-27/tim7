import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  selector: 'app-games-home',
  imports: [RouterLink],
  template: `
    <div class="stack">
      <div class="page-header">
        <div>
          <span class="badge badge--games">Games</span>
          <h1>Games</h1>
          <p>
            Map-based challenges: answer a quiz at a landmark or find a hidden location from clues.
          </p>
        </div>
      </div>

<nav class="subnav" aria-label="Games pages">
        <a routerLink="/games/challenges">Social challenges</a>
        @if (isAdministrator()) {
          <a routerLink="/games/location-challenges">Lokacijski izazovi</a>
        }
      </nav>

      <div class="empty stack">
        <h2>No games yet</h2>
        <p>Games will appear here once authors publish them. Meanwhile, explore a tour.</p>
        <p><a class="button" routerLink="/exploration">Browse tours</a></p>
      </div>
    </div>
  `,
})
export class GamesHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
