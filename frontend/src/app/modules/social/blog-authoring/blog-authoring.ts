import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BlogDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

// Creating and editing a blog send a multipart form, because image files cannot travel in JSON.
// The field names match the parameters of the server's actions.
@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  async create(title: string, description: string, images: File[]): Promise<BlogDto> {
    const body = new FormData();
    body.append('title', title);
    body.append('description', description);
    for (const image of images) {
      body.append('images', image);
    }
    return firstValueFrom(this.http.post<BlogDto>(BASE_URL, body));
  }

  async edit(
    id: string,
    title: string,
    description: string,
    keptImages: string[],
    newImages: File[],
  ): Promise<void> {
    const body = new FormData();
    body.append('title', title);
    body.append('description', description);
    for (const address of keptImages) {
      body.append('keptImages', address);
    }
    for (const image of newImages) {
      body.append('newImages', image);
    }
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, body));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }
}
