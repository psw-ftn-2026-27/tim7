import { Component, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { Map, MapPin } from '../../shared/map/map';
import { PositionSimulator } from '../../shared/position-simulator/position-simulator';

// A development page that shows every way of embedding the shared map. Nothing is saved.
const PETROVARADIN: MapPin = {
  latitude: 45.2517,
  longitude: 19.8628,
  label: 'Petrovaradin Fortress',
};
const CITY_HALL: MapPin = { latitude: 45.2551, longitude: 19.8452, label: 'City Hall' };
const DANUBE_PARK: MapPin = { latitude: 45.2546, longitude: 19.8526, label: 'Danube Park' };
const LIBERTY_BRIDGE: MapPin = { latitude: 45.2447, longitude: 19.8517 };
const STRAND: MapPin = { latitude: 45.2427, longitude: 19.8445, label: 'Strand' };

@Component({
  imports: [FormField, Map, PositionSimulator],
  selector: 'app-map-demo',
  styleUrl: './map-demo.scss',
  templateUrl: './map-demo.html',
})
export class MapDemo {
  protected readonly route = [CITY_HALL, DANUBE_PARK, PETROVARADIN, LIBERTY_BRIDGE, STRAND];

  protected readonly sights = [PETROVARADIN, CITY_HALL, DANUBE_PARK];

  // Challenges in a city, as Games would show them: pins only, no path and no order.
  protected readonly challenges: MapPin[] = [
    { latitude: 45.2531, longitude: 19.8615, label: 'Quiz: the clock tower' },
    { latitude: 45.2569, longitude: 19.8441, label: 'Hidden location: the old well' },
    { latitude: 45.2486, longitude: 19.8398, label: 'Quiz: the Danube bridges' },
    { latitude: 45.2601, longitude: 19.8329, label: 'Hidden location: the secret garden' },
  ];

  protected readonly pathPins = signal(this.route.slice(0, 3));
  protected readonly pathLength = signal(0);

  protected readonly initialLocation: MapPin = { latitude: 45.2505, longitude: 19.8565 };
  protected readonly pickedWithout = signal<MapPin | null>(null);
  protected readonly pickedWith = signal<MapPin | null>(null);

  // Building a tour: the picked location becomes the next point of the path.
  protected readonly tourPins = signal<MapPin[]>([]);
  protected readonly tourPicked = signal<MapPin | null>(null);
  protected readonly tourLength = signal(0);

  // The position simulator: the last position it reported, undefined until the first load.
  protected readonly position = signal<MapPin | null | undefined>(undefined);

  // A location challenge, as Games would show it: the start point and the area around it.
  // Games computes the radius; the hidden locations are not shown.
  protected readonly challengeStart: MapPin = {
    latitude: 45.2551,
    longitude: 19.8452,
    label: 'Start: City Hall',
  };
  protected readonly challengePins = [this.challengeStart];
  protected readonly challengeRadiusKm = 0.6;

  // Location search: the centre comes from the map, the radius from the page's form.
  protected readonly searchCenter = signal<MapPin | null>(null);
  protected readonly searchModel = signal({ radiusKm: null as number | null });
  protected readonly searchForm = form(this.searchModel, (path) => {
    required(path.radiusKm, { message: 'Enter a radius.' });
    validate(path.radiusKm, ({ value }) => {
      const radiusKm = value();
      if (radiusKm !== null && radiusKm <= 0) {
        return { kind: 'notPositive', message: 'The radius must be greater than 0.' };
      }
      return null;
    });
  });
  // What the page would send to the server; a real page sends it and shows the results as pins.
  protected readonly searchSent = signal<{ center: MapPin; radiusKm: number } | null>(null);

  // Path to the next key point: the tourist is a plain pin labelled "You".
  protected readonly pathToNext: MapPin[] = [
    { latitude: 45.2486, longitude: 19.8502, label: 'You' },
    PETROVARADIN,
  ];
  protected readonly distanceToNext = signal(0);

  protected search(): void {
    const center = this.searchCenter();
    const radiusKm = this.searchModel().radiusKm;
    if (!center || radiusKm === null || !this.searchForm().valid()) {
      return;
    }
    this.searchSent.set({ center, radiusKm });
  }

  protected addToTour(): void {
    const picked = this.tourPicked();
    if (!picked) {
      return;
    }
    this.tourPins.set([...this.tourPins(), picked]);
    this.tourPicked.set(null);
  }

  protected addPoint(): void {
    this.pathPins.set(this.route.slice(0, this.pathPins().length + 1));
  }

  protected removePoint(): void {
    this.pathPins.set(this.route.slice(0, this.pathPins().length - 1));
  }
}
