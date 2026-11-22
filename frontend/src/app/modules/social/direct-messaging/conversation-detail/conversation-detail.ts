import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Chat, ChatMessage, chatMessageForm } from '../../../../shared/chat/chat';
import { serverMessage } from '../../../../shared/util/server-message';
import { ConversationChatDto } from '../../api/social-api-types';
import { DirectMessaging } from '../direct-messaging';

@Component({
  imports: [Chat, RouterLink],
  selector: 'app-conversation-detail',
  styleUrl: './conversation-detail.scss',
  templateUrl: './conversation-detail.html',
})
export class ConversationDetail {
  private readonly directMessaging = inject(DirectMessaging);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly chat = httpResource<ConversationChatDto>(
    () => `/api/social/conversations/${this.id()}/chat`,
  );

  // Nobody is privileged in a conversation, so no message can be muted.
  protected readonly chatMessages = computed<ChatMessage[]>(() =>
    (this.chat.value()?.messages.items ?? []).map((message) => ({
      id: message.id,
      authorId: message.authorId,
      authorEmail: message.authorEmail ?? message.authorId,
      text: message.text,
      sentAt: message.sentAt,
      authorCanBeMuted: false,
    })),
  );

  protected readonly chatForm = chatMessageForm();
  protected readonly chatPending = signal(false);
  protected readonly chatError = signal<string | null>(null);

  constructor() {
    const timer = setInterval(() => this.chat.reload(), 5000);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  protected async sendMessage(text: string): Promise<void> {
    this.chatPending.set(true);
    this.chatError.set(null);
    try {
      await this.directMessaging.sendMessage(this.id(), { text });
      this.chatForm().reset({ text: '' });
    } catch (failure) {
      this.chatError.set(serverMessage(failure, 'Could not send the message.'));
    } finally {
      this.chatPending.set(false);
      this.chat.reload();
    }
  }
}
