import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserBalanceDto } from '../../api/payment-api-types';
import { WalletAdministration } from '../wallet-administration';

@Component({
  imports: [FormField],
  selector: 'app-user-wallets',
  styleUrl: './user-wallets.scss',
  templateUrl: './user-wallets.html',
})
export class UserWallets {
  private readonly walletAdministration = inject(WalletAdministration);

  protected readonly users = httpResource<PageResult<UserBalanceDto>>(
    () => '/api/payment/wallets?page=1&pageSize=20',
  );

  protected readonly selectedUserId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected readonly form = form(signal({ amount: null as number | null }), (path) => {
    required(path.amount, { message: 'Amount is required.' });
    min(path.amount, 1, { message: 'Amount must be at least 1.' });
    validate(path.amount, ({ value }) =>
      value() === null || Number.isInteger(value())
        ? null
        : { kind: 'integer', message: 'Amount must be a whole number.' },
    );
  });

  protected select(userId: string): void {
    this.error.set(null);
    this.success.set(null);
    this.selectedUserId.set(userId);
    this.form().reset({ amount: null });
  }

  protected async deposit(event: Event): Promise<void> {
    event.preventDefault();
    const userId = this.selectedUserId();
    const amount = this.form.amount().value();
    if (userId === null || amount === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.walletAdministration.deposit(userId, { amount });
      this.users.reload();
      this.form().reset({ amount: null });
      this.success.set(`Deposited ${amount} EC.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not deposit EC.'));
    } finally {
      this.pending.set(false);
    }
  }
}
