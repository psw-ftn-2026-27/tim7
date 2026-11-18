# Deljeni čet (`app-chat`)

Prikazna komponenta platformskog tima. Prikazuje formu za novu poruku i spisak poruka,
najnoviju na vrhu, uz e-mail autora i vreme slanja. Ne ubrizgava joj se nijedan servis, ne
razgovara sa serverom i samo javlja slanje kroz izlaz: poruke, osvežavanje i komandu drži
stranica modula koja je ugrađuje. Sva stanja su prikazana na razvojnoj stranici `/chat-demo`.

```ts
import { Chat, ChatMessage, chatMessageForm } from '../../../../shared/chat/chat'; // broj ../ zavisi od dubine stranice

@Component({ imports: [Chat], ... })
```

## Tip `ChatMessage`

```ts
interface ChatMessage {
  id: string;
  authorEmail: string;
  text: string;
  sentAt: string; // vreme koje je postavio server
}
```

`shared/` ne sme da uvozi tipove iz `api/` modula, pa stranica pravi poruke od DTO-a svog
modula, na primer u `computed()`:

```ts
protected readonly chatMessages = computed<ChatMessage[]>(() =>
  (this.messages.value()?.items ?? []).map((message) => ({
    id: message.id,
    authorEmail: message.authorEmail,
    text: message.text,
    sentAt: message.sentAt,
  })),
);
```

## Forma: `chatMessageForm()`

Funkcija pravi Signal Form sa poljem `text` i pravilom P1: poruka ne sme biti prazna (ni samo
razmaci) niti duža od 1000 karaktera. Stranica je poziva u inicijalizatoru polja i predaje je
komponenti:

```ts
protected readonly chatForm = chatMessageForm();
```

Komponenta formu ne prazni, jer ne zna ishod komande. Stranica to radi posle uspeha:

```ts
this.chatForm().reset({ text: '' });
```

## Ulazi

| Ulaz       | Tip                           | Podrazumevano | Značenje                                                                                     |
| ---------- | ----------------------------- | ------------- | -------------------------------------------------------------------------------------------- |
| `messages` | `ChatMessage[]`               | obavezan      | Poruke koje se prikazuju. Komponenta ih sama ređa po `sentAt`, najnovija na vrhu.            |
| `form`     | `FieldTree<ChatMessageModel>` | obavezan      | Forma iz `chatMessageForm()`. Greške validacije stoje ispod polja kada ga korisnik napusti.  |
| `locked`   | `boolean`                     | `false`       | Zaključan čet (P4): umesto forme stoji kratko obaveštenje, a poruke se i dalje vide.         |
| `error`    | `string \| null`              | `null`        | Poruka servera posle neuspešnog slanja; stoji ispod forme kao `.alert`.                                   |
| `pending`  | `boolean`                     | `false`       | Dok komanda traje, dugme „Send“ je onemogućeno. Onemogućeno je i dok forma nije validna.     |

## Izlazi

| Izlaz  | Tip      | Kada se javlja                                                                  |
| ------ | -------- | ------------------------------------------------------------------------------- |
| `send` | `string` | Kada korisnik pošalje validnu formu, a komanda ne traje. Nosi tekst poruke.     |

## Šta radi stranica

Stranica modula drži sve što ima veze sa serverom:

- poruke u **sopstvenom** `httpResource`, odvojenom od ostatka stranice;
- periodično osvežavanje tog resursa na svakih 5 sekundi, po obrascu „Periodic refresh“ iz
  `frontend/AGENTS.md` (`setInterval` u konstruktoru i `clearInterval` u
  `DestroyRef.onDestroy`). Dok traje osvežavanje, stare poruke ostaju vidljive, pa šablon
  proverava `hasValue()` pre `isLoading()`;
- komandu slanja kroz servis svoje grupe, sa signalima `pending` i `error`;
- posle uspeha prazni formu i ponovo učitava resurs poruka; posle neuspeha tekst ostaje u
  polju, a poruka servera ide u `error`;
- odluku da li je čet zaključan.

## Primer ugradnje

```html
@if (messages.hasValue()) {
  <app-chat
    [messages]="chatMessages()"
    [form]="chatForm"
    [locked]="locked()"
    [error]="chatError()"
    [pending]="chatPending()"
    (send)="sendMessage($event)"
  />
} @else if (messages.isLoading()) {
  <p>Loading...</p>
}
```

```ts
protected async sendMessage(text: string): Promise<void> {
  this.chatPending.set(true);
  this.chatError.set(null);
  try {
    await this.clubChat.sendMessage(this.id(), { text });
    this.chatForm().reset({ text: '' });
    this.messages.reload();
  } catch (failure) {
    this.chatError.set(serverMessage(failure, 'The message could not be sent.'));
  } finally {
    this.chatPending.set(false);
  }
}
```
