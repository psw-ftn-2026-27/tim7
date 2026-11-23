import { httpResource } from '@angular/common/http';
import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { FormField, form, min, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { Map, MapPin } from '../../../../shared/map/map';
import {
  TourDifficulty,
  TourOnMapDto,
  TourSearchDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';
import { TourCard } from '../tour-card/tour-card';

const PAGE_SIZE = 20;

@Component({
  imports: [FormField, Map, RouterLink, TourCard],
  selector: 'app-tour-map-search',
  styleUrl: './tour-map-search.scss',
  templateUrl: './tour-map-search.html',
})
export class TourMapSearch {
  private readonly tourBrowsing = inject(TourBrowsing);

  private readonly model = signal({
    name: '',
    easy: false,
    moderate: false,
    hard: false,
    tags: '',
    transport: '' as TransportMode | '',
    minPrice: null as number | null,
    maxPrice: null as number | null,
    minLength: null as number | null,
    maxLength: null as number | null,
    radiusKilometers: null as number | null,
  });

  // Every field is optional, so an empty field (null) is valid. Prices are whole EC.
  protected readonly form = form(this.model, (path) => {
    min(path.minPrice, 0, { message: 'Min price cannot be negative.' });
    min(path.maxPrice, 0, { message: 'Max price cannot be negative.' });
    validate(path.minPrice, ({ value }) => {
      const minPrice = value();
      if (minPrice !== null && !Number.isInteger(minPrice)) {
        return { kind: 'notWhole', message: 'Min price must be a whole number of EC.' };
      }
      return null;
    });
    validate(path.maxPrice, ({ value }) => {
      const maxPrice = value();
      if (maxPrice !== null && !Number.isInteger(maxPrice)) {
        return { kind: 'notWhole', message: 'Max price must be a whole number of EC.' };
      }
      return null;
    });
    validate(path.maxPrice, ({ value }) => {
      const maxPrice = value();
      const minPrice = this.model().minPrice;
      if (maxPrice !== null && minPrice !== null && maxPrice < minPrice) {
        return { kind: 'belowMin', message: 'Max price cannot be less than min price.' };
      }
      return null;
    });
    min(path.minLength, 0, { message: 'Min length cannot be negative.' });
    min(path.maxLength, 0, { message: 'Max length cannot be negative.' });
    validate(path.maxLength, ({ value }) => {
      const maxLength = value();
      const minLength = this.model().minLength;
      if (maxLength !== null && minLength !== null && maxLength < minLength) {
        return { kind: 'belowMin', message: 'Max length cannot be less than min length.' };
      }
      return null;
    });
    // `min` would let zero through, so the radius gets its own check.
    validate(path.radiusKilometers, ({ value }) => {
      const radius = value();
      if (radius !== null && radius <= 0) {
        return { kind: 'notPositive', message: 'Radius must be greater than 0 km.' };
      }
      return null;
    });
  });

  // The criteria the user entered, in the shape the server expects; null means "not set".
  // The map page has no sorting.
  private readonly search = computed<TourSearchDto>(() => {
    const criteria = this.model();

    const difficulties: TourDifficulty[] = [];
    if (criteria.easy) {
      difficulties.push('Easy');
    }
    if (criteria.moderate) {
      difficulties.push('Moderate');
    }
    if (criteria.hard) {
      difficulties.push('Hard');
    }

    const tags = criteria.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');

    return {
      name: criteria.name === '' ? null : criteria.name,
      difficulties: difficulties.length === 0 ? null : difficulties,
      tags: tags.length === 0 ? null : tags,
      transport: criteria.transport === '' ? null : criteria.transport,
      minPrice: criteria.minPrice,
      maxPrice: criteria.maxPrice,
      minLength: criteria.minLength,
      maxLength: criteria.maxLength,
      sortBy: null,
    };
  });

  // The point the user clicked on the map; null until the first click.
  protected readonly center = signal<MapPin | null>(null);

  // The radius of the circle the map draws around the point, read as the user types.
  // The map draws no circle for 0, so an empty field shows only the point.
  protected readonly circleRadiusKm = computed(() => this.model().radiusKilometers ?? 0);

  // The criteria of the last "Search", copied when the button is clicked. The list reads
  // only these, so typing in a filter or moving the point does not reload it.
  // When the page opens, the list shows the first page of all published tours.
  private readonly submitted = signal({
    search: this.search(),
    center: null as MapPin | null,
    radiusKilometers: null as number | null,
  });

  protected readonly page = signal(1);

  // Set by "Search" with a circle; the search is recorded once its results arrive.
  private readonly recordingPending = signal(false);

  // Reloads from the server when "Search" copies new criteria or the page changes.
  protected readonly tours = httpResource<PageResult<TourOnMapDto>>(() => {
    const { search, center, radiusKilometers } = this.submitted();
    const params = new URLSearchParams();
    params.set('page', String(this.page()));
    params.set('pageSize', String(PAGE_SIZE));
    if (search.name !== null) {
      params.set('name', search.name);
    }
    for (const difficulty of search.difficulties ?? []) {
      params.append('difficulties', difficulty);
    }
    for (const tag of search.tags ?? []) {
      params.append('tags', tag);
    }
    if (search.transport !== null) {
      params.set('transport', search.transport);
    }
    if (search.minPrice !== null) {
      params.set('minPrice', String(search.minPrice));
    }
    if (search.maxPrice !== null) {
      params.set('maxPrice', String(search.maxPrice));
    }
    if (search.minLength !== null) {
      params.set('minLength', String(search.minLength));
    }
    if (search.maxLength !== null) {
      params.set('maxLength', String(search.maxLength));
    }
    if (center !== null && radiusKilometers !== null) {
      params.set('latitude', String(center.latitude));
      params.set('longitude', String(center.longitude));
      params.set('radiusKilometers', String(radiusKilometers));
    }
    return `/api/exploration/tours/published/map?${params}`;
  });

  // A marker for every tour on the current page that has a public key point.
  protected readonly pins = computed<MapPin[]>(() => {
    if (!this.tours.hasValue()) {
      return [];
    }
    const pins: MapPin[] = [];
    for (const item of this.tours.value().items) {
      const location = item.firstPublicKeyPointLocation;
      if (location !== null) {
        pins.push({
          latitude: location.latitude,
          longitude: location.longitude,
          label: item.tour.name,
        });
      }
    }
    return pins;
  });

  protected readonly hasNextPage = computed(() => {
    if (!this.tours.hasValue()) {
      return false;
    }
    return this.page() * PAGE_SIZE < this.tours.value().totalCount;
  });

  constructor() {
    // The record needs the total count from the response, and httpResource tells only
    // through its signals when the response has arrived. So an effect watches the status and
    // sends the command once, after "Search" with a circle. Paging never sets the flag.
    effect(() => {
      const status = this.tours.status();
      if (!this.recordingPending() || (status !== 'resolved' && status !== 'error')) {
        return;
      }
      this.recordingPending.set(false);
      if (status === 'resolved') {
        untracked(() => this.recordSearch());
      }
    });
  }

  protected onLocationPicked(location: MapPin): void {
    this.center.set(location);
  }

  // Copies the entered criteria, so the list reloads from page 1. The search is recorded
  // only when both the point and the radius are set.
  protected runSearch(): void {
    const center = this.center();
    const radiusKilometers = this.model().radiusKilometers;
    this.submitted.set({ search: this.search(), center, radiusKilometers });
    this.page.set(1);
    this.recordingPending.set(center !== null && radiusKilometers !== null);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }

  private async recordSearch(): Promise<void> {
    const { search, center, radiusKilometers } = this.submitted();
    if (center === null || radiusKilometers === null || !this.tours.hasValue()) {
      return;
    }
    try {
      await this.tourBrowsing.recordLocationSearch({
        center: { latitude: center.latitude, longitude: center.longitude },
        radiusKilometers,
        name: search.name,
        difficulties: search.difficulties,
        tags: search.tags,
        transport: search.transport,
        minPrice: search.minPrice,
        maxPrice: search.maxPrice,
        minLength: search.minLength,
        maxLength: search.maxLength,
        resultCount: this.tours.value().totalCount,
      });
    } catch {
      // The record is for the platform, not for the user, so the results stay without a message.
    }
  }
}
