namespace Payment.Application.WalletAdministration;

public sealed record UserBalanceDto(Guid UserId, string Email, int Balance);
