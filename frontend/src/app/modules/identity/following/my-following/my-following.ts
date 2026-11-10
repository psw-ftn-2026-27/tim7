import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowRequestStatus, FollowRequestsDto, UserDto } from '../../api/identity-api-types';
import { Following } from '../following';

@Component({
  imports: [RouterLink],
  selector: 'app-my-following',
  styleUrl: './my-following.scss',
  templateUrl: './my-following.html',
})
export class MyFollowing {
  private readonly following = inject(Following);

  protected readonly requests = httpResource<FollowRequestsDto>(() => '/api/identity/requests');

  protected readonly followers = httpResource<PageResult<UserDto>>(
    () => '/api/identity/followers?page=1&pageSize=20',
  );

  protected readonly followed = httpResource<PageResult<UserDto>>(
    () => '/api/identity/following?page=1&pageSize=20',
  );

  protected readonly statusLabels: Record<FollowRequestStatus, string> = {
    Pending: 'Na čekanju',
    Accepted: 'Prihvaćen',
    Rejected: 'Odbijen',
  };

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(senderId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.acceptRequest(senderId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije prihvaćen.'));
    } finally {
      this.reloadAll();
      this.pending.set(false);
    }
  }

  protected async reject(senderId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.rejectRequest(senderId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije odbijen.'));
    } finally {
      this.reloadAll();
      this.pending.set(false);
    }
  }

  protected async withdraw(recipientId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.withdrawRequest(recipientId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije povučen.'));
    } finally {
      this.reloadAll();
      this.pending.set(false);
    }
  }

  private reloadAll(): void {
    this.requests.reload();
    this.followers.reload();
    this.followed.reload();
  }
}
