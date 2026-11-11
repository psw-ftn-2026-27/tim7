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

  readonly publish = output<void>();
  readonly retire = output<void>();
  readonly restore = output<void>();
  readonly archive = output<void>();

  protected readonly confirming = signal(false);

  protected canPublish(): boolean {
    return this.challenge().status === 'Draft';
  }

  protected canRetire(): boolean {
    return this.challenge().status === 'Published';
  }

  protected canRestore(): boolean {
    return this.challenge().status === 'Retired';
  }

  protected canArchive(): boolean {
    const status = this.challenge().status;
    return status === 'Published' || status === 'Retired';
  }

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
