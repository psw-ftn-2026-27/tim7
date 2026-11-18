import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/identity';

@Injectable({ providedIn: 'root' })
export class Following {
  private readonly http = inject(HttpClient);

  async sendRequest(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/requests/sent/${recipientId}`, null));
  }

  async withdrawRequest(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/requests/sent/${recipientId}`));
  }

  async acceptRequest(senderId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/requests/received/${senderId}/accept`, null),
    );
  }

  async rejectRequest(senderId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/requests/received/${senderId}/reject`, null),
    );
  }

  async removeFollower(followerId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/followers/${followerId}`));
  }

  async unfollow(followedId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/following/${followedId}`));
  }
}
