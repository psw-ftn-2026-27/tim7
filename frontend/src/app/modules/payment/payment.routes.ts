import { Routes } from '@angular/router';
import { PaymentHome } from './payment-home/payment-home';
import { MyCart } from './tour-purchasing/my-cart/my-cart';
import { TourOffer } from './tour-purchasing/tour-offer/tour-offer';
import { UserWallets } from './wallet-administration/user-wallets/user-wallets';
import { MyWallet } from './wallet-browsing/my-wallet/my-wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: MyWallet },
  { path: 'tours/:id', component: TourOffer },
  { path: 'cart', component: MyCart },
  { path: 'users', component: UserWallets },
];
