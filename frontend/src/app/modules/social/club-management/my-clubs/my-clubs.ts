import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubManagement } from '../club-management';

@Component({
  imports: [RouterLink],
  selector: 'app-my-clubs',
  styleUrl: './my-clubs.scss',
  templateUrl: './my-clubs.html',
})
export class MyClubs {
  private readonly clubManagement = inject(ClubManagement);

  protected readonly clubs = httpResource<ClubDto[]>(() => '/api/social/clubs/mine');

  // The club row that asks to confirm an action, and which action; one row at a time.
  protected readonly confirmingId = signal<string | null>(null);
  protected readonly confirmingAction = signal<'close' | 'delete' | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected askConfirm(clubId: string, action: 'close' | 'delete'): void {
    this.error.set(null);
    this.confirmingId.set(clubId);
    this.confirmingAction.set(action);
  }

  protected cancelConfirm(): void {
    this.confirmingId.set(null);
    this.confirmingAction.set(null);
  }

  // Closing cannot be undone; the table reloads whether the server accepts or rejects it.
  protected async confirmClose(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.close(clubId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not close the club.'));
    } finally {
      this.pending.set(false);
      this.cancelConfirm();
      this.clubs.reload();
    }
  }

  protected async confirmDelete(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.delete(clubId);
      this.cancelConfirm();
      this.clubs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
