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
      if (showPath) {
        this.pinLayer.addLayer(
          L.polyline(pins.map(toLatLng), {
            color: this.token('--color-accent'),
            weight: 4,
            opacity: 0.6,
            interactive: false,
          }),
        );
        this.pathLength.emit(pathLengthKm(pins));
      }
      pins.forEach((pin, index) => {
        this.pinLayer.addLayer(this.pinMarker(pin, showPath ? index + 1 : null, pins.length));
      });
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
    // The selected pin ignores clicks, so a click on it reaches the map and moves it.
    this.selectedMarker = location
      ? L.circleMarker(toLatLng(location), {
          radius: 11,
          color: this.token('--color-surface'),
          weight: 3,
          fillColor: this.token('--color-danger'),
          fillOpacity: 1,
          interactive: false,
        }).addTo(this.map)
      : null;
  }

  // A click on a pin opens its popup and does not reach the map, so it never picks a location.
  private pinMarker(pin: MapPin, order: number | null, count: number): L.CircleMarker {
    const marker = L.circleMarker(toLatLng(pin), {
      radius: 8,
      color: this.token('--color-surface'),
      weight: 3,
      fillColor: this.token('--color-accent'),
      fillOpacity: 1,
      bubblingMouseEvents: false,
    });
    marker.bindPopup(popupContent(pin, order, count), { offset: [0, -6] });
    if (pin.label) {
      marker.bindTooltip(textElement('span', pin.label), { direction: 'top', offset: [0, -8] });
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

// The popup shows the order (only with a path), the label and the coordinates.
function popupContent(pin: MapPin, order: number | null, count: number): HTMLElement {
  const content = document.createElement('div');
  if (order !== null) {
    content.append(textElement('div', `Point ${order} of ${count}`));
  }
  if (pin.label) {
    const line = document.createElement('div');
    line.append(textElement('strong', pin.label));
    content.append(line);
  }
  content.append(textElement('div', `${pin.latitude.toFixed(5)}, ${pin.longitude.toFixed(5)}`));
  return content;
}

// Leaflet treats a string as HTML, so labels written by users are passed as text in an element.
function textElement(tag: string, text: string): HTMLElement {
  const element = document.createElement(tag);
  element.textContent = text;
  return element;
}
