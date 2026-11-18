import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyInvitationDto } from '../../api/social-api-types';
import { ClubInvitations } from '../club-invitations';

@Component({
  imports: [RouterLink],
  selector: 'app-my-invitations',
  styleUrl: './my-invitations.scss',
  templateUrl: './my-invitations.html',
})
export class MyInvitations {
  private readonly clubInvitations = inject(ClubInvitations);

  protected readonly invitations = httpResource<MyInvitationDto[]>(
    () => '/api/social/clubs/invitations',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected accept(clubId: string): Promise<void> {
    return this.run(
      () => this.clubInvitations.acceptInvitation(clubId),
      'Could not accept the invitation.',
    );
  }

  protected reject(clubId: string): Promise<void> {
    return this.run(
      () => this.clubInvitations.rejectInvitation(clubId),
      'Could not reject the invitation.',
    );
  }

  // Runs one command, shows the server's message if it fails, and reloads the invitations either way.
  private async run(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await command();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.pending.set(false);
      this.invitations.reload();
    }
  }
}
