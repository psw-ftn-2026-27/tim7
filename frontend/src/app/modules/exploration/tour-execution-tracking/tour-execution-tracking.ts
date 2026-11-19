import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tour-executions';

@Injectable({ providedIn: 'root' })
export class TourExecutionTracking {
  private readonly http = inject(HttpClient);

  async abandon(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/abandon`, {}));
  }

  // Records the position in the active session and completes the next key point when it is near.
  async checkPosition(position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/position-checks`, position));
  }
}
