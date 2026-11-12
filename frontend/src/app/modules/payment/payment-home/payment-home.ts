import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-payment-home',
  template: `
    <div class="stack">
      <h1>Payment</h1>
      <p><a routerLink="/payment/wallet">My wallet</a></p>
      @if (isAdministrator()) {
        <p><a routerLink="/payment/users">Users and wallets</a></p>
      }
    </div>
  `,
})
export class PaymentHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
