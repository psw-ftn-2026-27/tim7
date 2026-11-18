import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/wallet';

@Injectable({ providedIn: 'root' })
export class WalletBrowsing {
  private readonly http = inject(HttpClient);

  async openWallet(): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, {}));
  }
}
