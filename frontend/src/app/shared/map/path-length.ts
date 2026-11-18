import type { MapPin } from './map';

const EARTH_RADIUS_KM = 6371;

/** Sum of great-circle (haversine) distances between consecutive pins, in km, unrounded. */
export function pathLengthKm(pins: MapPin[]): number {
  let total = 0;
  for (let i = 1; i < pins.length; i++) {
    total += distanceKm(pins[i - 1], pins[i]);
  }
  return total;
}

function distanceKm(from: MapPin, to: MapPin): number {
  const deltaLatitude = toRadians(to.latitude - from.latitude);
  const deltaLongitude = toRadians(to.longitude - from.longitude);
  const a =
    Math.sin(deltaLatitude / 2) ** 2 +
    Math.cos(toRadians(from.latitude)) *
      Math.cos(toRadians(to.latitude)) *
      Math.sin(deltaLongitude / 2) ** 2;
  return 2 * EARTH_RADIUS_KM * Math.asin(Math.sqrt(a));
}

function toRadians(degrees: number): number {
  return (degrees * Math.PI) / 180;
}
