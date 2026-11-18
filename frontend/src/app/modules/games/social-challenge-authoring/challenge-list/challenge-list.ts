import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { ChallengeCard } from '../challenge-card/challenge-card';
import {
  ChallengeForm,
  ChallengeFormValue,
  challengeRules,
  emptyChallenge,
} from '../challenge-form/challenge-form';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

// Minutes since midnight, from an "HH:mm:ss" time.
function minutesOf(time: string): number {
  const [hours, minutes] = time.split(':');
  return Number(hours) * 60 + Number(minutes);
}

function durationOf(challenge: SocialChallengeDto): number {
  return minutesOf(challenge.end) - minutesOf(challenge.start);
}

@Component({
  imports: [ChallengeCard, ChallengeForm, FormField, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly authoring = inject(SocialChallengeAuthoring);

  protected readonly challenges = httpResource<SocialChallengeDto[]>(() => '/api/games/challenges');

  protected readonly view = form(signal({ sort: 'name', status: 'all', duration: 'all' }));

  protected readonly visible = computed(() => {
    const { sort, status, duration } = this.view().value();
    const list = (this.challenges.value() ?? []).filter((challenge) => {
      const minutes = durationOf(challenge);
      return (
        (status === 'all' || challenge.status === status) &&
        (duration === 'all' ||
          (duration === 'short' && minutes < 120) ||
          (duration === 'medium' && minutes >= 120 && minutes <= 360) ||
          (duration === 'long' && minutes > 360))
      );
    });
    return list.sort((a, b) => {
      if (sort === 'duration') {
        return durationOf(a) - durationOf(b);
      }
      if (sort === 'people') {
        return a.people - b.people;
      }
      return a.name.localeCompare(b.name);
    });
  });

  protected readonly listError = signal<string | null>(null);

  protected readonly editingId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly editForm = form(
    signal<ChallengeFormValue>({ ...emptyChallenge }),
    challengeRules,
  );

  protected startEdit(challenge: SocialChallengeDto): void {
    this.editError.set(null);
    this.editingId.set(challenge.id);
    this.editForm().reset({
      name: challenge.name,
      start: challenge.start.slice(0, 5),
      end: challenge.end.slice(0, 5),
      people: challenge.people,
      rangeMeters: challenge.rangeMeters,
      xp: challenge.xp,
      exitMessage: challenge.exitMessage,
    });
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected async saveEdit(): Promise<void> {
    const id = this.editingId();
    if (id === null) {
      return;
    }
    this.pending.set(true);
    this.editError.set(null);
    try {
      const value = this.editForm().value();
      await this.authoring.update(id, {
        ...value,
        start: `${value.start}:00`,
        end: `${value.end}:00`,
      });
      this.editingId.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Izazov nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(id: string): Promise<void> {
    this.listError.set(null);
    try {
      await this.authoring.remove(id);
      this.challenges.reload();
    } catch (failure) {
      this.listError.set(serverMessage(failure, 'Izazov nije moguće obrisati.'));
    }
  }
}
