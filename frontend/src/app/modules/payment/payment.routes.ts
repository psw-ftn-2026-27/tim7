import { Routes } from '@angular/router';
import { PaymentHome } from './payment-home/payment-home';
import { MyWallet } from './wallet-browsing/my-wallet/my-wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: MyWallet },
];
