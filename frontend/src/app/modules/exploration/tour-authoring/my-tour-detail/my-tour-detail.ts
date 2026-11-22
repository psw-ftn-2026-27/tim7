import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Chat, ChatMessage, ChatMutedUser, chatMessageForm } from '../../../../shared/chat/chat';
import { ImagePicker } from '../../../../shared/image-picker/image-picker';
import { Map, MapPin } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { AuthorTourDto, KeyPointDto, TourChatDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

const EMPTY_KEY_POINT = {
  name: '',
  description: '',
  secret: '',
  isPublic: false,
  latitude: null as number | null,
  longitude: null as number | null,
};

@Component({
  imports: [Chat, FormField, ImagePicker, Map, RouterLink],
  selector: 'app-my-tour-detail',
  styleUrl: './my-tour-detail.scss',
  templateUrl: './my-tour-detail.html',
})
export class MyTourDetail {
  private readonly tourAuthoring = inject(TourAuthoring);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<AuthorTourDto>(
    () => `/api/exploration/tours/mine/${this.id()}`,
  );

  // The server returns the key points in their order, so the path follows the array.
  protected readonly pins = computed<MapPin[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((point) => ({
      latitude: point.location.latitude,
      longitude: point.location.longitude,
      label: point.name,
    })),
  );

  // The form is open while adding (no key point) or while changing a key point.
  protected readonly formOpen = signal(false);
  protected readonly editing = signal<KeyPointDto | null>(null);

  // While changing a key point the map starts with the selected pin on its location.
  protected readonly initialLocation = computed<MapPin | null>(() => {
    const point = this.editing();
    return point === null
      ? null
      : { latitude: point.location.latitude, longitude: point.location.longitude };
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);

  // A draft tour has no chat, so no request is sent for it. The chat of an archived tour is locked.
  protected readonly chat = httpResource<TourChatDto>(() => {
    const status = this.tour.value()?.status;
    return status === undefined || status === 'Draft'
      ? undefined
      : `/api/exploration/tours/${this.id()}/chat`;
  });

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

  private readonly model = signal({ ...EMPTY_KEY_POINT });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.latitude, { message: 'Pick the location on the map.' });
    required(path.longitude, { message: 'Pick the location on the map.' });
  });

  protected readonly imageFiles = signal<File[]>([]);
  protected readonly imagesValid = signal(true);

  // A key point has exactly one image: a new key point needs one, a change may keep the current one.
  protected readonly tooManyImages = computed(() => this.imageFiles().length > 1);
  protected readonly imageMissing = computed(
    () => this.editing() === null && this.imageFiles().length === 0,
  );

  protected readonly canSave = computed(
    () =>
      this.form().valid() &&
      this.imagesValid() &&
      !this.tooManyImages() &&
      !this.imageMissing() &&
      !this.pending(),
  );

  constructor() {
    const timer = setInterval(() => this.chat.reload(), 5000);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  protected async sendMessage(text: string): Promise<void> {
    const sent = await this.runChat(
      () => this.tourAuthoring.sendMessage(this.id(), { text }),
      'Could not send the message.',
    );
    if (sent) {
      this.chatForm().reset({ text: '' });
    }
  }

  protected deleteMessage(messageId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourAuthoring.deleteMessage(this.id(), messageId),
      'Could not delete the message.',
    );
  }

  protected muteAuthor(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourAuthoring.mute(this.id(), userId),
      'Could not mute the participant.',
    );
  }

  protected unmuteUser(userId: string): Promise<boolean> {
    return this.runChat(
      () => this.tourAuthoring.unmute(this.id(), userId),
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

  protected startAdd(): void {
    this.formError.set(null);
    this.editing.set(null);
    this.form().reset({ ...EMPTY_KEY_POINT });
    this.formOpen.set(true);
  }

  protected startEdit(point: KeyPointDto): void {
    this.formError.set(null);
    this.editing.set(point);
    this.form().reset({
      name: point.name,
      description: point.description,
      secret: point.secret ?? '',
      isPublic: point.isPublic,
      latitude: point.location.latitude,
      longitude: point.location.longitude,
    });
    this.formOpen.set(true);
  }

  protected onLocationPicked(location: MapPin): void {
    this.model.update((value) => ({
      ...value,
      latitude: location.latitude,
      longitude: location.longitude,
    }));
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    if (latitude === null || longitude === null) {
      return;
    }
    const secret = this.form.secret().value();
    const dto = {
      name: this.form.name().value(),
      description: this.form.description().value(),
      location: { latitude, longitude },
      secret: secret === '' ? null : secret,
      isPublic: this.form.isPublic().value(),
    };
    const image = this.imageFiles()[0] ?? null;
    const point = this.editing();
    this.pending.set(true);
    this.formError.set(null);
    try {
      if (point !== null) {
        await this.tourAuthoring.changeKeyPoint(this.id(), point.id, dto, image);
      } else if (image !== null) {
        await this.tourAuthoring.addKeyPoint(this.id(), dto, image);
      }
      this.formOpen.set(false);
      this.editing.set(null);
      this.imageFiles.set([]);
      this.imagesValid.set(true);
      this.tour.reload();
    } catch (failure) {
      this.formError.set(serverMessage(failure, 'Could not save the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(point: KeyPointDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.removeKeyPoint(this.id(), point.id);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the key point.'));
    } finally {
      this.tour.reload();
      this.pending.set(false);
    }
  }
}
