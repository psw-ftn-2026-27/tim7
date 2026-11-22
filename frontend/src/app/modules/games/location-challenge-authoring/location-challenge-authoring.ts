import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateLocationChallengeDto } from '../api/games-api-types';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeAuthoring {
  private readonly http = inject(HttpClient);

  // The photo at position i of `images` belongs to the hidden point at position i.
  async create(dto: CreateLocationChallengeDto, images: File[]): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, locationChallengeFormData(dto, images)));
  }
}

function locationChallengeFormData(dto: CreateLocationChallengeDto, images: File[]): FormData {
  const body = new FormData();
  body.append('Name', dto.name);
  body.append('Xp', String(dto.xp));
  if (dto.startLocation !== null) {
    body.append('StartLocation.Latitude', String(dto.startLocation.latitude));
    body.append('StartLocation.Longitude', String(dto.startLocation.longitude));
  }
  (dto.hiddenPoints ?? []).forEach((point, index) => {
    if (point.location !== null) {
      body.append(`HiddenPoints[${index}].Location.Latitude`, String(point.location.latitude));
      body.append(`HiddenPoints[${index}].Location.Longitude`, String(point.location.longitude));
    }
  });
  for (const image of images) {
    body.append('images', image);
  }
  return body;
}
