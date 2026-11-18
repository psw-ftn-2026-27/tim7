export interface TransactionDto {
  amount: number;
  createdAt: string;
  administratorId: string | null;
  isReversed: boolean;
  kind: 'Deposit' | 'Purchase' | 'Earning';
  entityId: string | null;
}

export interface TourOfferDto {
  name: string;
  price: number | null;
}

export interface CartItemDto {
  entityId: string;
  name: string;
  price: number;
}

export interface CartDto {
  items: CartItemDto[];
  totalPrice: number;
}

export interface WalletDto {
  balance: number;
  transactions: TransactionDto[];
}

export interface UserBalanceDto {
  userId: string;
  email: string;
  balance: number;
}

export interface DepositDto {
  amount: number;
}

export interface UserDepositDto {
  id: string;
  amount: number;
  createdAt: string;
  administratorId: string;
  isReversed: boolean;
}
