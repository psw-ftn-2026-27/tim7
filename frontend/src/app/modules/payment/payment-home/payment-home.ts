import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-payment-home',
  template: `
    <div class="stack">
      <div class="page-header">
        <div>
          <span class="badge badge--payment">Payment</span>
          <h1>Payment</h1>
          <p>Your wallet and, for administrators, the wallets of all users.</p>
        </div>
      </div>
      <nav class="subnav" aria-label="Payment pages">
        <a routerLink="/payment/wallet">My wallet</a>
        @if (isAdministrator()) {
          <a routerLink="/payment/users">Users and wallets</a>
        }
      </nav>
    </div>
  `,
})
export class PaymentHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
