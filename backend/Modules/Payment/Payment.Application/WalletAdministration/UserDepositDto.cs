namespace Payment.Application.WalletAdministration;

public sealed record UserDepositDto(Guid Id, int Amount, DateTime CreatedAt, Guid AdministratorId, bool IsReversed);
