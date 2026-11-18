import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Chat, ChatMessage, ChatMutedUser, chatMessageForm } from '../../../../shared/chat/chat';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubChatDto, ClubMembershipDto, ExplorerDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership';

@Component({
  imports: [Chat, FormField, RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubMembership = inject(ClubMembership);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubMembershipDto>(
    () => `/api/social/clubs/${this.id()}`,
  );

  protected readonly searchForm = form(signal({ username: '' }));

  // Stays null until the first search, so the page loads no explorers before that.
  private readonly searchedUsername = signal<string | null>(null);

  protected readonly explorers = httpResource<ExplorerDto[]>(() => {
    const username = this.searchedUsername();
    if (username === null) {
      return undefined;
    }
    return `/api/social/clubs/${this.id()}/explorers?username=${encodeURIComponent(username)}`;
  });

  // The member row that asks to confirm an action, and which action; one row at a time.
  protected readonly confirmingId = signal<string | null>(null);
  protected readonly confirmingAction = signal<'promote' | 'transfer' | 'remove' | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The chat has its own resource, so refreshing it every few seconds does not reload the club.
  // Only the owner and the members read it; for everyone else no request is sent.
  protected readonly chat = httpResource<ClubChatDto>(() =>
    this.detail.value()?.myState === 'Member' ? `/api/social/clubs/${this.id()}/chat` : undefined,
  );

  protected readonly chatMessages = computed<ChatMessage[]>(() =>
    (this.chat.value()?.messages.items ?? []).map((message) => ({
      id: message.id,
      authorId: message.authorId,
      authorEmail: message.authorEmail ?? message.authorId,
      text: message.text,
      sentAt: message.sentAt,
      authorCanBeMuted: message.authorCanBeMuted,
    })),
  );

  protected readonly chatMutedUsers = computed<ChatMutedUser[]>(() =>
    (this.chat.value()?.mutedUsers ?? []).map((user) => ({
      userId: user.userId,
      email: user.email ?? user.userId,
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
    const sent = await this.runChat(
      () => this.clubMembership.sendMessage(this.id(), { text }),
      'Could not send the message.',
    );
    if (sent) {
      this.chatForm().reset({ text: '' });
    }
  }

  protected deleteMessage(messageId: string): Promise<boolean> {
    return this.runChat(
      () => this.clubMembership.deleteMessage(this.id(), messageId),
      'Could not delete the message.',
    );
  }

  protected muteAuthor(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.clubMembership.mute(this.id(), userId),
      'Could not mute the member.',
    );
  }

  protected unmuteUser(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.clubMembership.unmute(this.id(), userId),
      'Could not lift the mute.',
    );
  }

  // Runs one chat command, shows the server's message if it fails, and reloads the chat either way.
  private async runChat(command: () => Promise<unknown>, fallback: string): Promise<boolean> {
    this.chatPending.set(true);
    this.chatError.set(null);
    try {
      await command();
      return true;
    } catch (failure) {
      this.chatError.set(serverMessage(failure, fallback));
      return false;
    } finally {
      this.chatPending.set(false);
      this.chat.reload();
    }
  }

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

  protected search(event: Event): void {
    event.preventDefault();
    this.error.set(null);
    this.searchedUsername.set(this.searchForm.username().value());
  }

  protected invite(userId: string): Promise<void> {
    return this.run(
      () => this.clubMembership.invite(this.id(), userId),
      'Could not send the invitation.',
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
