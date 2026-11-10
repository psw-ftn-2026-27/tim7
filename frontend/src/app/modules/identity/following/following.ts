import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/identity/requests';

@Injectable({ providedIn: 'root' })
export class Following {
  private readonly http = inject(HttpClient);

  async sendRequest(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/sent/${recipientId}`, null));
  }

  async withdrawRequest(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/sent/${recipientId}`));
  }

  async acceptRequest(senderId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/received/${senderId}/accept`, null));
  }

  async rejectRequest(senderId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/received/${senderId}/reject`, null));
  }
}
