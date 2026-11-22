import { Component, computed, inject, signal } from '@angular/core';
import {
  FormField,
  form,
  maxLength,
  min,
  minLength,
  required,
  validate,
} from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImagePicker } from '../../../../shared/image-picker/image-picker';
import { Map, MapPin } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

// The most photos the picker (and the server) accepts, so the most hidden points.
const MAX_POINTS = 10;

// One hidden point: a picked photo and the location chosen for it on the map.
interface PointRow {
  file: File;
  location: MapPin | null;
}

@Component({
  imports: [FormField, ImagePicker, Map],
  selector: 'app-create-location-challenge',
  styleUrl: './create-location-challenge.scss',
  templateUrl: './create-location-challenge.html',
})
export class CreateLocationChallenge {
  private readonly authoring = inject(LocationChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly maxPoints = MAX_POINTS;

  protected readonly form = form(signal({ name: '', xp: null as number | null }), (path) => {
    required(path.name, { message: 'Naziv je obavezan.' });
    minLength(path.name, 3, { message: 'Naziv ima najmanje 3 karaktera.' });
    maxLength(path.name, 20, { message: 'Naziv ima najviše 20 karaktera.' });
    required(path.xp, { message: 'XP je obavezan.' });
    min(path.xp, 1, { message: 'XP mora biti pozitivan.' });
    validate(path.xp, ({ value }) =>
      value() === null || Number.isInteger(value())
        ? null
        : { kind: 'integer', message: 'XP mora biti ceo broj.' },
    );
  });

  protected readonly startLocation = signal<MapPin | null>(null);
  protected readonly rows = signal<PointRow[]>([]);
  protected readonly imagesValid = signal(true);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly canSave = computed(
    () =>
      this.form().valid() &&
      this.startLocation() !== null &&
      this.rows().length > 0 &&
      this.rows().every((row) => row.location !== null) &&
      this.imagesValid() &&
      !this.pending(),
  );

  protected onStartPicked(location: MapPin): void {
    this.startLocation.set(location);
  }

  // One row per picked photo; a photo that stays keeps the location already chosen for it.
  protected onFilesChange(files: File[]): void {
    this.rows.update((rows) =>
      files.map((file) => ({
        file,
        location: rows.find((row) => row.file === file)?.location ?? null,
      })),
    );
  }

  protected onPointPicked(file: File, location: MapPin): void {
    this.rows.update((rows) => rows.map((row) => (row.file === file ? { ...row, location } : row)));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const rows = this.rows();
      await this.authoring.create(
        {
          name: this.form.name().value(),
          xp: this.form.xp().value() ?? 0,
          startLocation: this.startLocation(),
          hiddenPoints: rows.map((row) => ({ location: row.location })),
        },
        rows.map((row) => row.file),
      );
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async cancel(): Promise<void> {
    await this.router.navigate(['/games/location-challenges']);
  }
}
