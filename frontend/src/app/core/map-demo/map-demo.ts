import { Component, signal } from '@angular/core';
import { Map, MapPin } from '../../shared/map/map';

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
  imports: [Map],
  selector: 'app-map-demo',
  styleUrl: './map-demo.scss',
  templateUrl: './map-demo.html',
})
export class MapDemo {
  protected readonly route = [CITY_HALL, DANUBE_PARK, PETROVARADIN, LIBERTY_BRIDGE, STRAND];

  protected readonly sights = [PETROVARADIN, CITY_HALL, DANUBE_PARK];

  protected readonly pathPins = signal(this.route.slice(0, 3));
  protected readonly pathLength = signal(0);

  protected readonly initialLocation: MapPin = { latitude: 45.2505, longitude: 19.8565 };
  protected readonly pickedWithout = signal<MapPin | null>(null);
  protected readonly pickedWith = signal<MapPin | null>(null);

  // Building a tour: the picked location becomes the next point of the path.
  protected readonly tourPins = signal<MapPin[]>([]);
  protected readonly tourPicked = signal<MapPin | null>(null);
  protected readonly tourLength = signal(0);

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
