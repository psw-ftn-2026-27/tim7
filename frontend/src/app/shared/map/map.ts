import {
  Component,
  ElementRef,
  OnDestroy,
  effect,
  inject,
  input,
  output,
  untracked,
} from '@angular/core';
import * as L from 'leaflet';
import { pathLengthKm } from './path-length';

export interface MapPin {
  latitude: number;
  longitude: number;
  label?: string;
}

const NOVI_SAD: L.LatLngTuple = [45.2671, 19.8335];
const DEFAULT_ZOOM = 13;

@Component({
  selector: 'app-map',
  styleUrl: './map.scss',
  templateUrl: './map.html',
})
export class Map implements OnDestroy {
  readonly pins = input<MapPin[]>([]);
  readonly showPath = input(false);
  readonly picking = input(false);
  readonly initialLocation = input<MapPin | null>(null);

  readonly pathLength = output<number>();
  readonly locationPicked = output<MapPin>();

  // Platform-approved deviation from the Angular subset: Leaflet draws into the host
  // element, so every <app-map> gets its own map. Modules must not copy this.
  private readonly host: HTMLElement = inject(ElementRef).nativeElement;
  private readonly map = L.map(this.host);
  private readonly pinLayer = L.layerGroup().addTo(this.map);
  private selectedMarker: L.CircleMarker | null = null;

  constructor() {
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => {
      if (!this.picking()) {
        return;
      }
      const location = { latitude: event.latlng.lat, longitude: event.latlng.lng };
      this.placeSelectedMarker(location);
      this.locationPicked.emit(location);
    });

    // Leaflet lives outside the component tree, so an effect redraws it when the inputs change.
    // Pins and path: redraw them, report the length, and frame the view.
    effect(() => {
      const pins = this.pins();
      const showPath = this.showPath();
      this.pinLayer.clearLayers();
      for (const pin of pins) {
        this.pinLayer.addLayer(this.marker(pin, false));
      }
      if (showPath) {
        this.pinLayer.addLayer(
          L.polyline(pins.map(toLatLng), { color: this.token('--color-accent') }),
        );
        this.pathLength.emit(pathLengthKm(pins));
      }
      untracked(() => this.frameView());
    });

    // Picking: show the selected pin at the initial location, or no selected pin without one.
    effect(() => {
      this.placeSelectedMarker(this.picking() ? this.initialLocation() : null);
    });
  }

  ngOnDestroy(): void {
    this.map.remove();
  }

  private placeSelectedMarker(location: MapPin | null): void {
    this.selectedMarker?.remove();
    this.selectedMarker = location ? this.marker(location, true).addTo(this.map) : null;
  }

  private marker(pin: MapPin, selected: boolean): L.CircleMarker {
    const color = this.token(selected ? '--color-danger' : '--color-accent');
    const marker = L.circleMarker(toLatLng(pin), {
      radius: selected ? 10 : 7,
      color,
      fillColor: color,
      fillOpacity: selected ? 0.9 : 0.5,
      weight: 2,
    });
    if (pin.label) {
      marker.bindTooltip(pin.label, { permanent: true, direction: 'top', offset: [0, -8] });
    }
    return marker;
  }

  // Picking centres on the initial location, or on the default centre without one.
  // Otherwise the view frames every pin.
  private frameView(): void {
    const initialLocation = this.initialLocation();
    let points = this.pins().map(toLatLng);
    if (this.picking()) {
      points = initialLocation ? [toLatLng(initialLocation)] : [];
    }
    if (points.length === 0) {
      this.map.setView(NOVI_SAD, DEFAULT_ZOOM);
    } else if (points.length === 1) {
      this.map.setView(points[0], DEFAULT_ZOOM);
    } else {
      this.map.fitBounds(L.latLngBounds(points), { padding: [24, 24] });
    }
  }

  // Leaflet draws SVG outside the component's styles, so colours are read from the tokens.
  private token(name: string): string {
    return getComputedStyle(this.host).getPropertyValue(name).trim();
  }
}

function toLatLng(pin: MapPin): L.LatLngTuple {
  return [pin.latitude, pin.longitude];
}
