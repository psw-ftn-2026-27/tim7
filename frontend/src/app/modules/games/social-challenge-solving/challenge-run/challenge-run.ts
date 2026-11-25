import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapPin } from '../../../../shared/map/map';
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';
import { repeatEvery } from '../../../../shared/util/repeat-every';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeRunDto } from '../../api/games-api-types';
import { SocialChallengeSolving } from '../social-challenge-solving';

@Component({
  imports: [PositionSimulator, RouterLink],
  selector: 'app-challenge-run',
  styleUrl: './challenge-run.scss',
  templateUrl: './challenge-run.html',
})
export class ChallengeRun {
  private readonly solving = inject(SocialChallengeSolving);

  readonly id = input.required<string>();

  // The active participant count while the challenge is unsolved, the result once it is solved.
  protected readonly run = httpResource<SocialChallengeRunDto>(
    () => `/api/games/social-challenges/solving/${this.id()}`,
  );

  // The last position the simulator reported; sent on every check.
  protected readonly position = signal<MapPin | null>(null);
  // The server message above the participant count.
  protected readonly error = signal<string | null>(null);

  constructor() {
    repeatEvery(10_000, () => this.check());
  }

  private async check(): Promise<void> {
    const position = this.position();
    // Once the result arrives, the page stops sending the position.
    if (!position || this.run.value()?.result) {
      return;
    }
    try {
      await this.solving.reportPosition(this.id(), {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      this.error.set(null);
      this.run.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Pozicija nije poslata.'));
    }
  }
}
