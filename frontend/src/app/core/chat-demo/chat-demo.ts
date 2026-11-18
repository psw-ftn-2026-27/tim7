import { Component, signal } from '@angular/core';
import { Chat, ChatMessage, chatMessageForm } from '../../shared/chat/chat';

// A development page that shows every state of the shared chat. Nothing is sent to the server.
const MESSAGES: ChatMessage[] = [
  {
    id: '1',
    authorEmail: 'ana@example.com',
    text: 'Is anyone joining the fortress tour on Saturday?',
    sentAt: '2026-11-16T09:15:00Z',
  },
  {
    id: '2',
    authorEmail: 'marko@example.com',
    text: 'I am. Meeting at the clock tower at ten?',
    sentAt: '2026-11-16T09:42:00Z',
  },
  {
    id: '3',
    authorEmail: 'ana@example.com',
    text: 'Ten works.\nI will bring the map.',
    sentAt: '2026-11-17T18:05:00Z',
  },
];

@Component({
  imports: [Chat],
  selector: 'app-chat-demo',
  styleUrl: './chat-demo.scss',
  templateUrl: './chat-demo.html',
})
export class ChatDemo {
  protected readonly messages = MESSAGES;

  // Open chat: sending adds the message here instead of on the server.
  protected readonly openMessages = signal<ChatMessage[]>(MESSAGES);
  protected readonly openForm = chatMessageForm();

  // Locked chat: the form is passed but not shown.
  protected readonly lockedForm = chatMessageForm();

  // Server error: every send fails with the message a server would return.
  protected readonly failingForm = chatMessageForm();
  protected readonly failingError = signal<string | null>(null);

  private nextId = MESSAGES.length + 1;

  protected sendOpen(text: string): void {
    const message: ChatMessage = {
      id: String(this.nextId++),
      authorEmail: 'you@example.com',
      text,
      sentAt: new Date().toISOString(),
    };
    this.openMessages.set([...this.openMessages(), message]);
    // As a real page does after the server accepts the message.
    this.openForm().reset({ text: '' });
  }

  protected sendFailing(): void {
    // As a real page does when the server rejects the message: the text stays in the field.
    this.failingError.set('The chat is locked.');
  }
}
