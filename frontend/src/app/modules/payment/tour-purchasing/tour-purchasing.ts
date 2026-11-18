import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/cart';

@Injectable({ providedIn: 'root' })
export class TourPurchasing {
  private readonly http = inject(HttpClient);

  async addToCart(entityId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/items/${entityId}`, null));
  }

  async removeFromCart(entityId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/items/${entityId}`));
  }

  async pay(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/payment`, null));
  }
}
