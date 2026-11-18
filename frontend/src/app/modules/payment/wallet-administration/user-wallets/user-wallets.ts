import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
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

  private readonly pageSize = 20;

  protected readonly page = signal(1);
  // The text that was searched for in the e-mails; null while the full list is shown.
  protected readonly searchedEmail = signal<string | null>(null);

  protected readonly searchForm = form(signal({ email: '' }));

  // One resource reads both the full list and the search results: both return one page of users.
  protected readonly users = httpResource<PageResult<UserBalanceDto>>(() => {
    const email = this.searchedEmail();
    const paging = `page=${this.page()}&pageSize=${this.pageSize}`;
    return email === null
      ? `/api/payment/wallets?${paging}`
      : `/api/payment/wallets/search?query=${encodeURIComponent(email)}&${paging}`;
  });

  protected readonly lastPage = computed(() =>
    Math.max(1, Math.ceil((this.users.value()?.totalCount ?? 0) / this.pageSize)),
  );

  protected readonly selectedUser = signal<UserBalanceDto | null>(null);

  // The URL stays undefined until a user is selected, so the read does not fire before that.
  protected readonly deposits = httpResource<UserDepositDto[]>(() => {
    const user = this.selectedUser();
    return user === null ? undefined : `/api/payment/wallets/${user.userId}/deposits`;
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

  protected search(event: Event): void {
    event.preventDefault();
    const email = this.searchForm.email().value().trim();
    if (email === '') {
      return;
    }
    this.page.set(1);
    this.searchedEmail.set(email);
  }

  protected clearSearch(): void {
    this.page.set(1);
    this.searchedEmail.set(null);
    this.searchForm().reset({ email: '' });
  }

  protected previousPage(): void {
    this.page.update((current) => current - 1);
  }

  protected nextPage(): void {
    this.page.update((current) => current + 1);
  }

  protected select(user: UserBalanceDto): void {
    this.error.set(null);
    this.reverseError.set(null);
    this.success.set(null);
    this.selectedUser.set(user);
    this.form().reset({ amount: null });
  }

  protected async deposit(event: Event): Promise<void> {
    event.preventDefault();
    const user = this.selectedUser();
    const amount = this.form.amount().value();
    if (user === null || amount === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.walletAdministration.deposit(user.userId, { amount });
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
    const user = this.selectedUser();
    if (user === null) {
      return;
    }
    this.pending.set(true);
    this.reverseError.set(null);
    this.success.set(null);
    try {
      await this.walletAdministration.reverse(user.userId, transactionId);
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
