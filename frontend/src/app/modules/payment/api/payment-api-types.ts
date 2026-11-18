export interface TransactionDto {
  amount: number;
  createdAt: string;
  administratorId: string;
  isReversed: boolean;
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
