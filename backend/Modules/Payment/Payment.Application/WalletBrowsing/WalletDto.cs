namespace Payment.Application.WalletBrowsing;

public sealed record WalletDto(int Balance, List<TransactionDto> Transactions);
