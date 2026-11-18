using Payment.Domain.Wallets;

namespace Payment.Application.WalletBrowsing;

// AdministratorId is set for a deposit, EntityId for a purchase and an earning.
public sealed record TransactionDto(int Amount, DateTime CreatedAt, Guid? AdministratorId, bool IsReversed, TransactionKind Kind, Guid? EntityId);
