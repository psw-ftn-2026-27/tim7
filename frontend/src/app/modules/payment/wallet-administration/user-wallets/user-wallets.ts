import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserBalanceDto, UserDepositDto } from '../../api/payment-api-types';
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

  // The URL stays undefined until a user is selected, so the read does not fire before that.
  protected readonly deposits = httpResource<UserDepositDto[]>(() => {
    const userId = this.selectedUserId();
    return userId === null ? undefined : `/api/payment/wallets/${userId}/deposits`;
  });
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly reverseError = signal<string | null>(null);
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
    this.reverseError.set(null);
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
      this.deposits.reload();
      this.form().reset({ amount: null });
      this.success.set(`Deposited ${amount} EC.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not deposit EC.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reverse(transactionId: string): Promise<void> {
    const userId = this.selectedUserId();
    if (userId === null) {
      return;
    }
    this.pending.set(true);
    this.reverseError.set(null);
    this.success.set(null);
    try {
      await this.walletAdministration.reverse(userId, transactionId);
      this.users.reload();
      this.deposits.reload();
      this.success.set('Deposit reversed.');
    } catch (failure) {
      this.reverseError.set(serverMessage(failure, 'Could not reverse the deposit.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }
}
