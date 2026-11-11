namespace Payment.Application.WalletBrowsing;

public sealed record TransactionDto(int Amount, DateTime CreatedAt, Guid AdministratorId, bool IsReversed);
