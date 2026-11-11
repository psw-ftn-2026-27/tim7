import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';
import { Map, MapPin } from './map';
import { pathLengthKm } from './path-length';

// One degree of longitude on the equator is about 111.19 km.
const ORIGIN: MapPin = { latitude: 0, longitude: 0 };
const ONE_EAST: MapPin = { latitude: 0, longitude: 1 };
const ONE_DEGREE_KM = 111.19;

function createMap(inputs: Record<string, unknown>): ComponentFixture<Map> {
  const fixture = TestBed.createComponent(Map);
  for (const [name, value] of Object.entries(inputs)) {
    fixture.componentRef.setInput(name, value);
  }
  fixture.detectChanges();
  return fixture;
}

// The component keeps its Leaflet map private, so the test catches it when it adds a layer.
function leafletMapOf(addLayer: ReturnType<typeof spyOnAddLayer>): L.Map {
  return addLayer.mock.contexts[0] as L.Map;
}

function spyOnAddLayer() {
  return vi.spyOn(L.Map.prototype, 'addLayer');
}

function markerPositions(map: L.Map): string[] {
  const positions: string[] = [];
  map.eachLayer((layer) => {
    if (layer instanceof L.CircleMarker) {
      const { lat, lng } = layer.getLatLng();
      positions.push(`${lat},${lng}`);
    }
  });
  return positions.sort();
}

function click(map: L.Map, latitude: number, longitude: number): void {
  map.fire('click', { latlng: L.latLng(latitude, longitude) });
}

describe('Map', () => {
  afterEach(() => vi.restoreAllMocks());

  it('sums the distances between consecutive pins rather than first to last, and is 0 for zero or one pin', () => {
    expect(pathLengthKm([ORIGIN, ONE_EAST, ORIGIN])).toBeCloseTo(2 * ONE_DEGREE_KM, 1);
    expect(pathLengthKm([])).toBe(0);
    expect(pathLengthKm([ORIGIN])).toBe(0);
  });

  it('reports a new length after the pins change', () => {
    const fixture = createMap({ pins: [ORIGIN, ONE_EAST], showPath: true });
    const lengths: number[] = [];
    fixture.componentInstance.pathLength.subscribe((length) => lengths.push(length));

    fixture.componentRef.setInput('pins', [ORIGIN, ONE_EAST, ORIGIN]);
    fixture.detectChanges();

    expect(lengths.at(-1)).toBeCloseTo(2 * ONE_DEGREE_KM, 1);
  });

  it('does not report a length when the path is off', () => {
    const fixture = createMap({ pins: [ORIGIN, ONE_EAST] });
    const lengths: number[] = [];
    fixture.componentInstance.pathLength.subscribe((length) => lengths.push(length));

    fixture.componentRef.setInput('pins', [ORIGIN, ONE_EAST, ORIGIN]);
    fixture.detectChanges();

    expect(lengths).toEqual([]);
  });

  it('in picking mode shows the selected pin at the initial location, and a click reports the new coordinates and moves only the selected pin', () => {
    const addLayer = spyOnAddLayer();
    const fixture = createMap({
      pins: [ORIGIN, ONE_EAST],
      picking: true,
      initialLocation: { latitude: 1, longitude: 1 },
    });
    const map = leafletMapOf(addLayer);
    const picked: MapPin[] = [];
    fixture.componentInstance.locationPicked.subscribe((location) => picked.push(location));

    expect(markerPositions(map)).toEqual(['0,0', '0,1', '1,1']);

    click(map, 2, 2);

    expect(picked).toEqual([{ latitude: 2, longitude: 2 }]);
    expect(markerPositions(map)).toEqual(['0,0', '0,1', '2,2']);
  });

  it('outside picking mode a click reports no coordinates', () => {
    const addLayer = spyOnAddLayer();
    const fixture = createMap({ pins: [ORIGIN] });
    const map = leafletMapOf(addLayer);
    const picked: MapPin[] = [];
    fixture.componentInstance.locationPicked.subscribe((location) => picked.push(location));

    click(map, 2, 2);

    expect(picked).toEqual([]);
  });
});
