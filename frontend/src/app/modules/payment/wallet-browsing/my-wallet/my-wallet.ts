import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { WalletDto } from '../../api/payment-api-types';
import { WalletBrowsing } from '../wallet-browsing';

@Component({
  selector: 'app-my-wallet',
  styleUrl: './my-wallet.scss',
  templateUrl: './my-wallet.html',
})
export class MyWallet {
  private readonly walletBrowsing = inject(WalletBrowsing);

  protected readonly opened = signal(false);
  protected readonly openFailed = signal(false);

  // The URL stays undefined until the wallet is opened, so the read waits for the command.
  protected readonly wallet = httpResource<WalletDto>(() =>
    this.opened() ? '/api/payment/wallet' : undefined,
  );

  constructor() {
    this.open();
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }

  private async open(): Promise<void> {
    try {
      await this.walletBrowsing.openWallet();
      this.opened.set(true);
    } catch {
      this.openFailed.set(true);
    }
  }
}
