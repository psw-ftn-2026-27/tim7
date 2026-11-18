import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubMembership {
  private readonly http = inject(HttpClient);

  async requestToJoin(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/join-requests`, null));
  }

  async withdrawRequest(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/join-requests/mine`));
  }

  async acceptRequest(id: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/join-requests/${userId}/accept`, null),
    );
  }

  async rejectRequest(id: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/join-requests/${userId}/reject`, null),
    );
  }

  async promoteToModerator(id: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/members/${userId}/promote`, null),
    );
  }

  async transferOwnership(id: string, userId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/members/${userId}/transfer-ownership`, null),
    );
  }

  async removeMember(id: string, userId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/members/${userId}`));
  }
}
