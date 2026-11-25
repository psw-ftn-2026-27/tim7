import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MapPin } from '../../../../shared/map/map';
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';
import { serverMessage } from '../../../../shared/util/server-message';
import { SolvableSocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeSolving } from '../social-challenge-solving';

@Component({
  imports: [PositionSimulator],
  selector: 'app-solvable-challenge-list',
  styleUrl: './solvable-challenge-list.scss',
  templateUrl: './solvable-challenge-list.html',
})
export class SolvableChallengeList {
  private readonly solving = inject(SocialChallengeSolving);
  private readonly router = inject(Router);

  protected readonly challenges = httpResource<SolvableSocialChallengeDto[]>(
    () => '/api/games/social-challenges/solving',
  );

  // The last position the simulator reported; sent only when the explorer clicks "Aktiviraj".
  protected readonly position = signal<MapPin | null>(null);
  protected readonly pending = signal(false);
  // The server message above the list.
  protected readonly error = signal<string | null>(null);

  protected async activate(id: string): Promise<void> {
    const position = this.position();
    if (!position) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.solving.activate(id, {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      await this.router.navigate(['/games/social-challenges', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov nije moguće aktivirati.'));
      this.challenges.reload();
    } finally {
      this.pending.set(false);
    }
  }
}
