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
