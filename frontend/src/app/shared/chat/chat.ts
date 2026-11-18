import { Component, computed, input, output, signal } from '@angular/core';
import { FieldTree, FormField, form, maxLength, validate } from '@angular/forms/signals';

// A message as the chat shows it. The page builds it from its own module's DTO.
export interface ChatMessage {
  id: string;
  authorId: string;
  authorEmail: string;
  text: string;
  sentAt: string;
  // Whether a privileged viewer may mute the author; the page takes it from its module's DTO.
  authorCanBeMuted: boolean;
}

// A muted user as the chat's privileged participants see it.
export interface ChatMutedUser {
  userId: string;
  email: string;
}

export interface ChatMessageModel {
  text: string;
}

// The same limit the server checks (P1).
const MAX_TEXT_LENGTH = 1000;

let nextChatNumber = 1;

// Creates the form for a new message. The page calls it in a field initializer, passes the
// result to the chat, and resets it after the server accepts the message.
export function chatMessageForm(): FieldTree<ChatMessageModel> {
  return form(signal<ChatMessageModel>({ text: '' }), (path) => {
    validate(path.text, ({ value }) => {
      if (value().trim() === '') {
        return { kind: 'blank', message: 'Message is required.' };
      }
      return null;
    });
    maxLength(path.text, MAX_TEXT_LENGTH, {
      message: `Message cannot be longer than ${MAX_TEXT_LENGTH} characters.`,
    });
  });
}

@Component({
  imports: [FormField],
  selector: 'app-chat',
  styleUrl: './chat.scss',
  templateUrl: './chat.html',
})
export class Chat {
  readonly messages = input.required<ChatMessage[]>();
  readonly locked = input(false);
  readonly error = input<string | null>(null);
  readonly pending = input(false);
  readonly form = input.required<FieldTree<ChatMessageModel>>();
  readonly currentUserId = input<string | null>(null);
  readonly privileged = input(false);
  readonly muted = input(false);
  readonly mutedUsers = input<ChatMutedUser[]>([]);

  // Carries the text of a valid message; the page sends it to the server.
  readonly send = output<string>();
  // Carry the ID of the message to delete, of the author to mute and of the user to unmute.
  readonly delete = output<string>();
  readonly mute = output<string>();
  readonly unmute = output<string>();

  // The message whose deletion waits for confirmation, since a deletion cannot be undone.
  protected readonly confirmingId = signal<string | null>(null);

  // A page may show more than one chat, so every chat gets its own id for the field's label.
  protected readonly fieldId = `chat-message-${nextChatNumber++}`;

  // The newest message is on top, next to the form it was sent from.
  protected readonly newestFirst = computed(() =>
    [...this.messages()].sort(
      (first, second) => new Date(second.sentAt).getTime() - new Date(first.sentAt).getTime(),
    ),
  );

  protected submit(event: Event): void {
    event.preventDefault();
    const form = this.form();
    if (!form().valid() || this.pending()) {
      return;
    }
    this.send.emit(form.text().value());
  }

  protected canDelete(message: ChatMessage): boolean {
    return this.privileged() && message.authorId !== this.currentUserId();
  }

  protected confirmDelete(messageId: string): void {
    this.confirmingId.set(null);
    this.delete.emit(messageId);
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }
}
