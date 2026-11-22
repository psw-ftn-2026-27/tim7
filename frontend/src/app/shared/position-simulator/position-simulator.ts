import { httpResource } from '@angular/common/http';
import { Component, computed, effect, inject, input, output, signal, untracked } from '@angular/core';
import { PositionDto } from '../api/position-dto';
import { Map, MapPin } from '../map/map';
import { serverMessage } from '../util/server-message';
import { PositionSimulation } from './position-simulation';

// Platform-approved deviation, see README.md: this section reads and saves the position
// itself instead of leaving that to the page. Modules must not copy this.
@Component({
  imports: [Map],
  selector: 'app-position-simulator',
  styleUrl: './position-simulator.scss',
  templateUrl: './position-simulator.html',
})
export class PositionSimulator {
  readonly pins = input<MapPin[]>([]);

  readonly positionChanged = output<MapPin | null>();

  private readonly positionSimulation = inject(PositionSimulation);

  // The server answers 204 without a body when there is no position, so the value is null.
  protected readonly position = httpResource<PositionDto | null>(() => '/api/identity/position');

  // The saved position is the map's selected pin, so it looks different from the other pins.
  protected readonly savedLocation = computed<MapPin | null>(() => {
    const position = this.position.value();
    return position ? { latitude: position.latitude, longitude: position.longitude } : null;
  });

  protected readonly draft = signal<MapPin | null>(null);
  protected readonly pending = signal(false);
  protected readonly saved = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    // The page lives outside this component, so an effect reports the position to it
    // after the first load and after every reload that follows a confirmation.
    effect(() => {
      if (this.position.status() !== 'resolved') {
        return;
      }
      const location = this.savedLocation();
      untracked(() => this.positionChanged.emit(location));
    });
  }

  protected onLocationPicked(location: MapPin): void {
    this.draft.set(location);
    this.saved.set(false);
    this.error.set(null);
  }

  protected async confirm(): Promise<void> {
    const draft = this.draft();
    if (!draft) {
      return;
    }
    this.pending.set(true);
    this.saved.set(false);
    this.error.set(null);
    try {
      await this.positionSimulation.setPosition({
        latitude: draft.latitude,
        longitude: draft.longitude,
      });
      this.draft.set(null);
      this.saved.set(true);
      this.position.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the position.'));
    } finally {
      this.pending.set(false);
    }
  }
}
