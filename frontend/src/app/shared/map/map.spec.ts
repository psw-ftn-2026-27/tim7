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

function popupTexts(map: L.Map): string[] {
  const texts: string[] = [];
  map.eachLayer((layer) => {
    const content = layer instanceof L.CircleMarker ? layer.getPopup()?.getContent() : undefined;
    if (content instanceof HTMLElement) {
      texts.push([...content.children].map((line) => line.textContent).join(' | '));
    }
  });
  return texts.sort();
}

// Each circle on the map as its centre and its radius in metres.
function circles(map: L.Map): string[] {
  const found: string[] = [];
  map.eachLayer((layer) => {
    if (layer instanceof L.Circle) {
      const { lat, lng } = layer.getLatLng();
      found.push(`${lat},${lng} ${layer.getRadius()} m`);
    }
  });
  return found;
}

// Each path line on the map as its points, in order.
function paths(map: L.Map): string[][] {
  const found: string[][] = [];
  map.eachLayer((layer) => {
    if (layer instanceof L.Polyline) {
      found.push((layer.getLatLngs() as L.LatLng[]).map(({ lat, lng }) => `${lat},${lng}`));
    }
  });
  return found;
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

  it('with a path a pin shows its order, and without a path it does not', () => {
    const addLayer = spyOnAddLayer();
    createMap({ pins: [ORIGIN, ONE_EAST], showPath: true });
    createMap({ pins: [ORIGIN, ONE_EAST] });
    const [withPath, withoutPath] = [...new Set(addLayer.mock.contexts)] as L.Map[];

    expect(popupTexts(withPath)).toEqual([
      'Point 1 of 2 | 0.00000, 0.00000',
      'Point 2 of 2 | 0.00000, 1.00000',
    ]);
    expect(popupTexts(withoutPath)).toEqual(['0.00000, 0.00000', '0.00000, 1.00000']);
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

  it('draws the circle only with a centre and a radius greater than 0', () => {
    const addLayer = spyOnAddLayer();
    createMap({ circleRadiusKm: 1 });
    createMap({ circleCenter: ORIGIN, circleRadiusKm: 0 });
    createMap({ circleCenter: ORIGIN, circleRadiusKm: 1 });
    const [withoutCenter, withoutRadius, withBoth] = [...new Set(addLayer.mock.contexts)] as L.Map[];

    expect(circles(withoutCenter)).toEqual([]);
    expect(circles(withoutRadius)).toEqual([]);
    expect(circles(withBoth)).toEqual(['0,0 1000 m']);
  });

  it('turns a radius of 3 km into a circle of 3000 m', () => {
    const addLayer = spyOnAddLayer();
    createMap({ circleCenter: ORIGIN, circleRadiusKm: 3 });
    const map = leafletMapOf(addLayer);

    expect(circles(map)).toEqual(['0,0 3000 m']);
  });

  it('after the centre or the radius changes there is still exactly one circle, with the new values', () => {
    const addLayer = spyOnAddLayer();
    const fixture = createMap({ circleCenter: ORIGIN, circleRadiusKm: 1 });
    const map = leafletMapOf(addLayer);

    fixture.componentRef.setInput('circleCenter', ONE_EAST);
    fixture.detectChanges();

    expect(circles(map)).toEqual(['0,1 1000 m']);

    fixture.componentRef.setInput('circleRadiusKm', 2);
    fixture.detectChanges();

    expect(circles(map)).toEqual(['0,1 2000 m']);
  });

  it('with a user position the path starts at the user, but the length counts only the pins', () => {
    const addLayer = spyOnAddLayer();
    const fixture = createMap({ pins: [ONE_EAST], userPosition: ORIGIN, showPath: true });
    const map = leafletMapOf(addLayer);
    const lengths: number[] = [];
    fixture.componentInstance.pathLength.subscribe((length) => lengths.push(length));

    fixture.componentRef.setInput('pins', [ONE_EAST, ORIGIN]);
    fixture.detectChanges();

    expect(paths(map)).toEqual([['0,0', '0,1', '0,0']]);
    expect(lengths.at(-1)).toBeCloseTo(ONE_DEGREE_KM, 1);
  });

  it('in picking mode a click moves the circle to the selected pin', () => {
    const addLayer = spyOnAddLayer();
    const fixture = createMap({
      picking: true,
      initialLocation: { latitude: 1, longitude: 1 },
      circleRadiusKm: 1,
    });
    const map = leafletMapOf(addLayer);

    expect(circles(map)).toEqual(['1,1 1000 m']);

    click(map, 2, 2);
    fixture.detectChanges();

    expect(circles(map)).toEqual(['2,2 1000 m']);
  });
});
