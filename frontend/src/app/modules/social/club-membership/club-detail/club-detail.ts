import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubMembershipDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership';

@Component({
  imports: [RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubMembership = inject(ClubMembership);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubMembershipDto>(
    () => `/api/social/clubs/${this.id()}`,
  );

  // The member row that asks to confirm an action, and which action; one row at a time.
  protected readonly confirmingId = signal<string | null>(null);
  protected readonly confirmingAction = signal<'promote' | 'transfer' | 'remove' | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected requestToJoin(): Promise<void> {
    return this.run(
      () => this.clubMembership.requestToJoin(this.id()),
      'Could not send the join request.',
    );
  }

  protected withdrawRequest(): Promise<void> {
    return this.run(
      () => this.clubMembership.withdrawRequest(this.id()),
      'Could not withdraw the join request.',
    );
  }

  protected acceptRequest(userId: string): Promise<void> {
    return this.run(
      () => this.clubMembership.acceptRequest(this.id(), userId),
      'Could not accept the join request.',
    );
  }

  protected rejectRequest(userId: string): Promise<void> {
    return this.run(
      () => this.clubMembership.rejectRequest(this.id(), userId),
      'Could not reject the join request.',
    );
  }

  protected askConfirm(userId: string, action: 'promote' | 'transfer' | 'remove'): void {
    this.error.set(null);
    this.confirmingId.set(userId);
    this.confirmingAction.set(action);
  }

  protected cancelConfirm(): void {
    this.confirmingId.set(null);
    this.confirmingAction.set(null);
  }

  protected confirm(userId: string): Promise<void> {
    const action = this.confirmingAction();
    this.cancelConfirm();
    if (action === 'promote') {
      return this.run(
        () => this.clubMembership.promoteToModerator(this.id(), userId),
        'Could not make the member a moderator.',
      );
    }
    if (action === 'transfer') {
      return this.run(
        () => this.clubMembership.transferOwnership(this.id(), userId),
        'Could not transfer ownership.',
      );
    }
    return this.run(
      () => this.clubMembership.removeMember(this.id(), userId),
      'Could not remove the member.',
    );
  }

  // Runs one command, shows the server's message if it fails, and reloads the club either way.
  private async run(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await command();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.pending.set(false);
      this.detail.reload();
    }
  }
}
