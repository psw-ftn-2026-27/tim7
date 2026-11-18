import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BlogDto, CreateBlogDto, EditBlogDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateBlogDto, images: File[]): Promise<BlogDto> {
    const body = blogFormData(dto.title, dto.description, images);
    return firstValueFrom(this.http.post<BlogDto>(BASE_URL, body));
  }

  async edit(id: string, dto: EditBlogDto, images: File[]): Promise<void> {
    const body = blogFormData(dto.title, dto.description, images);
    for (const address of dto.keptImages ?? []) {
      body.append('KeptImages', address);
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

// A blog carries files, so it travels as multipart/form-data instead of JSON:
// every DTO field under the name the server binds, and the images as files next to them.
function blogFormData(title: string, description: string, images: File[]): FormData {
  const body = new FormData();
  body.append('Title', title);
  body.append('Description', description);
  for (const image of images) {
    body.append('images', image);
  }
  return body;
}
