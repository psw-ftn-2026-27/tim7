import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { RecordLocationSearchDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);

  async recordLocationSearch(dto: RecordLocationSearchDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/location-searches`, dto));
  }
}
