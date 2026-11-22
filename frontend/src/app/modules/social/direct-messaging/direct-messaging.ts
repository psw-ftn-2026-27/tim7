import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendMessageDto, StartConversationDto } from '../api/social-api-types';

const BASE_URL = '/api/social/conversations';

@Injectable({ providedIn: 'root' })
export class DirectMessaging {
  private readonly http = inject(HttpClient);

  // Returns the conversation of the pair: the existing one or a new one.
  async start(dto: StartConversationDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async sendMessage(id: string, dto: SendMessageDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${id}/chat/messages`, dto));
  }
}
