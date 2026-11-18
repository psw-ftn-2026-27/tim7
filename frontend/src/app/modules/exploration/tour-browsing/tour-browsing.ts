import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { RecordLocationSearchDto, StartTourDto } from '../api/exploration-api-types';

// Starting a tour is served by the backend's TourExecutionTracking group,
// but the command runs from the tour detail page, so it lives in this group's service.
@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);

  // Returns the id of the session: a new one, or the existing one when this tour is already active.
  async startTour(dto: StartTourDto): Promise<string> {
    return firstValueFrom(this.http.post<string>('/api/exploration/tour-executions', dto));
  }

  async recordLocationSearch(dto: RecordLocationSearchDto): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/exploration/tours/location-searches', dto));
  }
}
