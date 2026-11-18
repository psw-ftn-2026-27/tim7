import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Map, MapPin } from '../../../../shared/map/map';
import { PublishedTourDto } from '../../api/exploration-api-types';

@Component({
  imports: [Map, RouterLink],
  selector: 'app-tour-detail',
  styleUrl: './tour-detail.scss',
  templateUrl: './tour-detail.html',
})
export class TourDetail {
  readonly id = input.required<string>();

  // The server sends only the key points that are visible without purchase, without their secrets.
  protected readonly tour = httpResource<PublishedTourDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  // Only some of the key points are shown, so the map gets pins without a path.
  protected readonly pins = computed<MapPin[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((point) => ({
      latitude: point.location.latitude,
      longitude: point.location.longitude,
      label: point.name,
    })),
  );
}
