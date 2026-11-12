namespace Payment.Application.WalletAdministration;

public sealed record UserBalanceDto(Guid UserId, int Balance);
