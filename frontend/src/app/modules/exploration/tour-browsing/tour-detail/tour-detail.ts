import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Map, MapPin } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyActiveTourExecutionDto, PublishedTourDto } from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

@Component({
  imports: [Map, RouterLink],
  selector: 'app-tour-detail',
  styleUrl: './tour-detail.scss',
  templateUrl: './tour-detail.html',
})
export class TourDetail {
  private readonly tourBrowsing = inject(TourBrowsing);
  private readonly router = inject(Router);

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
    () =>
      this.position() !== null && this.active.value()?.execution === null && !this.pending(),
  );

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
