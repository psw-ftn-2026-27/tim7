import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/social-challenges/solving';

@Injectable({ providedIn: 'root' })
export class SocialChallengeSolving {
  private readonly http = inject(HttpClient);

  async activate(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, position));
  }

  async reportPosition(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/position`, position));
  }
}
