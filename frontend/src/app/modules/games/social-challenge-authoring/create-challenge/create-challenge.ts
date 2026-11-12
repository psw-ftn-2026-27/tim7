import { Component, inject, signal } from '@angular/core';
import { form } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ChallengeForm,
  ChallengeFormValue,
  challengeRules,
  emptyChallenge,
} from '../challenge-form/challenge-form';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [ChallengeForm],
  selector: 'app-create-challenge',
  styleUrl: './create-challenge.scss',
  templateUrl: './create-challenge.html',
})
export class CreateChallenge {
  private readonly authoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal<ChallengeFormValue>({ ...emptyChallenge }), challengeRules);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async save(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const value = this.form().value();
      await this.authoring.create({
        ...value,
        start: `${value.start}:00`,
        end: `${value.end}:00`,
      });
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async cancel(): Promise<void> {
    await this.router.navigate(['/games/challenges']);
  }
}
