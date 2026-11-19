import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Map, MapPin } from '../../../../shared/map/map';
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';
import { repeatEvery } from '../../../../shared/util/repeat-every';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyActiveTourExecutionDto } from '../../api/exploration-api-types';
import { TourExecutionTracking } from '../tour-execution-tracking';

@Component({
  imports: [Map, PositionSimulator, RouterLink],
  selector: 'app-active-tour',
  styleUrl: './active-tour.scss',
  templateUrl: './active-tour.html',
})
export class ActiveTour {
  private readonly tourExecutionTracking = inject(TourExecutionTracking);
  private readonly router = inject(Router);

  // The server always answers 200; without an active session the execution is null.
  protected readonly active = httpResource<MyActiveTourExecutionDto>(
    () => '/api/exploration/tour-executions/active',
  );

  // The last position the position simulator reported; null until the tourist marks one.
  protected readonly position = signal<MapPin | null>(null);

  // The tour of the last check. When the session is gone after a check, the tour was completed.
  protected readonly checkedTourId = signal<string | null>(null);

  // The completed key point whose secret is open in the list.
  protected readonly openedKeyPointId = signal<string | null>(null);

  // The simulator shows the next key point; completed key points come without a location.
  protected readonly keyPoints = computed<MapPin[]>(() => {
    const next = this.active.value()?.execution?.nextKeyPoint;
    if (!next) {
      return [];
    }
    return [{ latitude: next.location.latitude, longitude: next.location.longitude, label: next.name }];
  });

  // The path goes from the position reported by the simulator to the next key point.
  protected readonly pathToNext = computed<MapPin[]>(() => {
    const pins: MapPin[] = [];
    const position = this.position();
    if (position) {
      pins.push({ latitude: position.latitude, longitude: position.longitude, label: 'You' });
    }
    return [...pins, ...this.keyPoints()];
  });

  protected readonly confirming = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    repeatEvery(10_000, () => this.check());
  }

  protected onPositionChanged(position: MapPin | null): void {
    this.position.set(position);
  }

  protected toggleSecret(keyPointId: string): void {
    this.openedKeyPointId.update((opened) => (opened === keyPointId ? null : keyPointId));
  }

  protected askToAbandon(): void {
    this.error.set(null);
    this.confirming.set(true);
  }

  protected cancel(): void {
    this.confirming.set(false);
  }

  protected async abandon(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourExecutionTracking.abandon();
      await this.router.navigate(['/exploration', tourId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not abandon the tour.'));
      this.confirming.set(false);
      this.active.reload();
    } finally {
      this.pending.set(false);
    }
  }

  // Runs every 10 seconds; skipped while there is no position or no active session.
  private async check(): Promise<void> {
    const position = this.position();
    const execution = this.active.hasValue() ? this.active.value().execution : null;
    if (!position || !execution) {
      return;
    }
    this.checkedTourId.set(execution.tourId);
    try {
      await this.tourExecutionTracking.checkPosition({
        latitude: position.latitude,
        longitude: position.longitude,
      });
      this.active.reload();
    } catch (failure) {
      // 404: the session is no longer active, so the reload shows the end of the tour.
      // Any other failure shows nothing; the next check comes in 10 seconds.
      if (failure instanceof HttpErrorResponse && failure.status === 404) {
        this.active.reload();
      }
    }
  }
}
