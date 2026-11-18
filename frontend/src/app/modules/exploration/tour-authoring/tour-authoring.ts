import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CreateTourDto,
  SaveKeyPointDto,
  TourDto,
  TourPriceDto,
  TransportTimeDto,
  SendMessageDto,
} from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string, dto: TourPriceDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, dto));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async reactivate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/reactivate`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }

  async addKeyPoint(id: string, dto: SaveKeyPointDto, image: File): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/key-points`, keyPointFormData(dto, image)),
    );
  }

  // Without a new image (null) the key point keeps its current one.
  async changeKeyPoint(
    id: string,
    keyPointId: string,
    dto: SaveKeyPointDto,
    image: File | null,
  ): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(
        `${BASE_URL}/${id}/key-points/${keyPointId}`,
        keyPointFormData(dto, image),
      ),
    );
  }

  async removeKeyPoint(id: string, keyPointId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/key-points/${keyPointId}`));
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

// A key point carries a file, so it travels as multipart/form-data instead of JSON:
// every field under the name the server binds, and the image as a file next to them.
function keyPointFormData(dto: SaveKeyPointDto, image: File | null): FormData {
  const body = new FormData();
  body.append('Name', dto.name);
  body.append('Description', dto.description);
  body.append('Location.Latitude', String(dto.location.latitude));
  body.append('Location.Longitude', String(dto.location.longitude));
  if (dto.secret !== null) {
    body.append('Secret', dto.secret);
  }
  body.append('IsPublic', String(dto.isPublic));
  if (image !== null) {
    body.append('image', image);
  }
  return body;
}
