import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Map, MapPin } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyActiveTourExecutionDto } from '../../api/exploration-api-types';
import { TourExecutionTracking } from '../tour-execution-tracking';

@Component({
  imports: [Map, RouterLink],
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

  // The path goes from the tourist's start position to the first key point not yet completed.
  protected readonly pins = computed<MapPin[]>(() => {
    const execution = this.active.value()?.execution;
    if (!execution) {
      return [];
    }
    const pins: MapPin[] = [
      {
        latitude: execution.startLocation.latitude,
        longitude: execution.startLocation.longitude,
        label: 'You are here',
      },
    ];
    if (execution.firstKeyPoint) {
      pins.push({
        latitude: execution.firstKeyPoint.location.latitude,
        longitude: execution.firstKeyPoint.location.longitude,
        label: execution.firstKeyPoint.name,
      });
    }
    return pins;
  });

  protected readonly confirming = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

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
}
