import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto } from '../../api/payment-api-types';
import { TourPurchasing } from '../tour-purchasing';

@Component({
  imports: [RouterLink],
  selector: 'app-my-cart',
  templateUrl: './my-cart.html',
})
export class MyCart {
  private readonly tourPurchasing = inject(TourPurchasing);

  protected readonly cart = httpResource<CartDto>(() => '/api/payment/cart');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly paid = signal(false);

  protected async remove(entityId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.paid.set(false);
    try {
      await this.tourPurchasing.removeFromCart(entityId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the tour from the cart.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async pay(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.paid.set(false);
    try {
      await this.tourPurchasing.pay();
      this.cart.reload();
      this.paid.set(true);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not pay the cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}
