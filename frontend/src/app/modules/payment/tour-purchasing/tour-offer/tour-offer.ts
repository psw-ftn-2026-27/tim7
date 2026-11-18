import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourOfferDto } from '../../api/payment-api-types';
import { TourPurchasing } from '../tour-purchasing';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-offer',
  templateUrl: './tour-offer.html',
})
export class TourOffer {
  private readonly tourPurchasing = inject(TourPurchasing);

  readonly id = input.required<string>();

  protected readonly offer = httpResource<TourOfferDto>(
    () => `/api/payment/tours/${this.id()}/offer`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly added = signal(false);

  protected async addToCart(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.added.set(false);
    try {
      await this.tourPurchasing.addToCart(this.id());
      this.offer.reload();
      this.added.set(true);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the tour to the cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}
