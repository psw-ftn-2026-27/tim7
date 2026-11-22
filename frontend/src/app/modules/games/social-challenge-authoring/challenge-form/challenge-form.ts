import { Component, input, output } from '@angular/core';
import {
  FieldTree,
  FormField,
  SchemaPathTree,
  max,
  maxLength,
  min,
  minLength,
  required,
} from '@angular/forms/signals';

// The values of the form. Times are "HH:mm", as the time input gives them.
export interface ChallengeFormValue {
  name: string;
  start: string;
  end: string;
  people: number;
  rangeMeters: number;
  xp: number;
  exitMessage: string;
}

export const emptyChallenge: ChallengeFormValue = {
  name: '',
  start: '',
  end: '',
  people: 2,
  rangeMeters: 2,
  xp: 1,
  exitMessage: '',
};

// The field rules shown under the fields. The rules that need several fields
// (start before end, range big enough for the people) are checked by the server.
export function challengeRules(path: SchemaPathTree<ChallengeFormValue>): void {
  required(path.name, { message: 'Naziv je obavezan.' });
  maxLength(path.name, 50, { message: 'Naziv ima najviše 50 karaktera.' });
  required(path.start, { message: 'Početak je obavezan.' });
  required(path.end, { message: 'Kraj je obavezan.' });
  min(path.people, 2, { message: 'Broj ljudi je od 2 do 100.' });
  max(path.people, 100, { message: 'Broj ljudi je od 2 do 100.' });
  min(path.rangeMeters, 2, { message: 'Opseg je od 2 do 50 metara.' });
  max(path.rangeMeters, 50, { message: 'Opseg je od 2 do 50 metara.' });
  min(path.xp, 1, { message: 'XP mora biti pozitivan.' });
  required(path.exitMessage, { message: 'Izlazna poruka je obavezna.' });
  minLength(path.exitMessage, 20, { message: 'Izlazna poruka ima najmanje 20 karaktera.' });
  maxLength(path.exitMessage, 200, { message: 'Izlazna poruka ima najviše 200 karaktera.' });
}

@Component({
  imports: [FormField],
  selector: 'app-challenge-form',
  styleUrl: './challenge-form.scss',
  templateUrl: './challenge-form.html',
})
export class ChallengeForm {
  readonly form = input.required<FieldTree<ChallengeFormValue>>();
  readonly error = input<string | null>(null);
  readonly pending = input(false);

  readonly save = output<void>();
  readonly dismiss = output<void>();

  protected submit(event: Event): void {
    event.preventDefault();
    this.save.emit();
  }
}
