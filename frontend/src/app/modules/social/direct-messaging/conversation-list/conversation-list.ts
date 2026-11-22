import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ConversationDto, FollowedUserDto } from '../../api/social-api-types';
import { DirectMessaging } from '../direct-messaging';

@Component({
  imports: [RouterLink],
  selector: 'app-conversation-list',
  styleUrl: './conversation-list.scss',
  templateUrl: './conversation-list.html',
})
export class ConversationList {
  private readonly directMessaging = inject(DirectMessaging);
  private readonly router = inject(Router);

  protected readonly conversations = httpResource<ConversationDto[]>(
    () => '/api/social/conversations',
  );

  // Only a user one follows can be messaged.
  protected readonly followedUsers = httpResource<FollowedUserDto[]>(
    () => '/api/social/conversations/followed-users',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async message(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.directMessaging.start({ otherUserId: userId });
      await this.router.navigate(['/social/messages', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not start the conversation.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }
}
