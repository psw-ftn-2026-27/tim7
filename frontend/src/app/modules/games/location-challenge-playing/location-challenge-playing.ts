import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/location-challenges/playing';

@Injectable({ providedIn: 'root' })
export class LocationChallengePlaying {
  private readonly http = inject(HttpClient);

  async activate(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, position));
  }

  async checkHiddenPoint(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/check`, position));
  }
}
