import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Chat, ChatMessage, ChatMutedUser, chatMessageForm } from '../../../../shared/chat/chat';
import { Map, MapPin } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  MyActiveTourExecutionDto,
  PublishedTourDto,
  TourChatDto,
} from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

@Component({
  imports: [Chat, Map, RouterLink],
  selector: 'app-tour-detail',
  styleUrl: './tour-detail.scss',
  templateUrl: './tour-detail.html',
})
export class TourDetail {
  private readonly tourBrowsing = inject(TourBrowsing);
  private readonly router = inject(Router);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  // The server sends only the key points that are visible without purchase, without their secrets.
  protected readonly tour = httpResource<PublishedTourDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  // The tourist's active session decides between "Start tour", "Continue tour" and a message.
  protected readonly active = httpResource<MyActiveTourExecutionDto>(
    () => '/api/exploration/tour-executions/active',
  );

  // Only some of the key points are shown, so the map gets pins without a path.
  protected readonly pins = computed<MapPin[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((point) => ({
      latitude: point.location.latitude,
      longitude: point.location.longitude,
      label: point.name,
    })),
  );

  // The position picked on the map; it is not kept after leaving the page.
  protected readonly position = signal<MapPin | null>(null);
  protected readonly pending = signal(false);
  protected readonly startError = signal<string | null>(null);

  protected readonly canStart = computed(
    () => this.position() !== null && this.active.value()?.execution === null && !this.pending(),
  );

  // The chat has its own resource, so refreshing it every few seconds does not reload the tour.
  protected readonly chat = httpResource<TourChatDto>(
    () => `/api/exploration/tours/${this.id()}/chat`,
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
      () => this.tourBrowsing.sendMessage(this.id(), { text }),
      'Could not send the message.',
    );
    if (sent) {
      this.chatForm().reset({ text: '' });
    }
  }

  protected deleteMessage(messageId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourBrowsing.deleteMessage(this.id(), messageId),
      'Could not delete the message.',
    );
  }

  protected muteAuthor(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourBrowsing.mute(this.id(), userId),
      'Could not mute the participant.',
    );
  }

  protected unmuteUser(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourBrowsing.unmute(this.id(), userId),
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

  protected onLocationPicked(location: MapPin): void {
    this.position.set(location);
  }

  // Whether the tour may be started (free or bought) is checked by the server.
  protected async start(): Promise<void> {
    const position = this.position();
    if (position === null) {
      return;
    }
    this.pending.set(true);
    this.startError.set(null);
    try {
      await this.tourBrowsing.startTour({
        tourId: this.id(),
        startLocation: { latitude: position.latitude, longitude: position.longitude },
      });
      await this.router.navigate(['/exploration/active']);
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'Could not start the tour.'));
      this.active.reload();
    } finally {
      this.pending.set(false);
    }
  }
}
