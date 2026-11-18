import { Component, input, output, signal } from '@angular/core';
import { SocialChallengeDto } from '../../api/games-api-types';

@Component({
  selector: 'app-challenge-card',
  styleUrl: './challenge-card.scss',
  templateUrl: './challenge-card.html',
})
export class ChallengeCard {
  readonly challenge = input.required<SocialChallengeDto>();

  readonly edit = output<void>();
  readonly remove = output<void>();

  protected readonly confirming = signal(false);

  protected canEdit(): boolean {
    return this.challenge().status === 'Draft';
  }

  protected canDelete(): boolean {
    const status = this.challenge().status;
    return status === 'Draft' || status === 'Archived';
  }

  protected confirm(): void {
    this.confirming.set(false);
    this.remove.emit();
  }
}
