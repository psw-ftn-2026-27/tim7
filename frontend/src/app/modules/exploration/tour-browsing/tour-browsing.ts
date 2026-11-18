import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendMessageDto, StartTourDto } from '../api/exploration-api-types';

// Starting a tour is served by the backend's TourExecutionTracking group,
// but the command runs from the tour detail page, so it lives in this group's service.
@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);

  // Returns the id of the session: a new one, or the existing one when this tour is already active.
  async startTour(dto: StartTourDto): Promise<string> {
    return firstValueFrom(this.http.post<string>('/api/exploration/tour-executions', dto));
  }

  async sendMessage(tourId: string, dto: SendMessageDto): Promise<string> {
    return firstValueFrom(
      this.http.post<string>(`/api/exploration/tours/${tourId}/chat/messages`, dto),
    );
  }

  async deleteMessage(tourId: string, messageId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(`/api/exploration/tours/${tourId}/chat/messages/${messageId}`),
    );
  }

  async mute(tourId: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`/api/exploration/tours/${tourId}/chat/muted/${userId}`, null),
    );
  }

  async unmute(tourId: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(`/api/exploration/tours/${tourId}/chat/muted/${userId}`),
    );
  }
}
