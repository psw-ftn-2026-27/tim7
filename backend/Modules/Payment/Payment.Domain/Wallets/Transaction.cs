using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public int Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? AdministratorId { get; private set; }
    public bool IsReversed { get; private set; }
    public TransactionKind Kind { get; private set; }
    public Guid? EntityId { get; private set; }

    private Transaction() { }

    internal Transaction(int amount, Guid administratorId) : base(Guid.NewGuid())
    {
        if (amount <= 0)
        {
            throw new DomainException("The deposit amount must be greater than zero.");
        }

        Amount = amount;
        AdministratorId = administratorId;
        CreatedAt = DateTime.UtcNow;
        Kind = TransactionKind.Deposit;
    }

    // A purchase is stored as a negative amount, so the balance stays the sum of the amounts.
    internal Transaction(TransactionKind kind, Guid entityId, int amount, DateTime now) : base(Guid.NewGuid())
    {
        if (kind == TransactionKind.Deposit)
        {
            throw new DomainException("A deposit is not made for an entity.");
        }
        if (amount <= 0)
        {
            throw new DomainException("The amount must be greater than zero.");
        }

        Amount = kind == TransactionKind.Purchase ? -amount : amount;
        EntityId = entityId;
        CreatedAt = now;
        Kind = kind;
    }

    internal void Reverse(DateTime now)
    {
        if (Kind != TransactionKind.Deposit)
        {
            throw new DomainException("Only a deposit can be reversed.");
        }

        if (now - CreatedAt > TimeSpan.FromHours(24))
        {
            throw new DomainException("The deposit can only be reversed within 24 hours of being made.");
        }

        if (IsReversed)
        {
            throw new DomainException("The deposit has already been reversed.");
        }

        IsReversed = true;
    }
}
