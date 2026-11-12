using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public int Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid AdministratorId { get; private set; }
    public bool IsReversed { get; private set; }

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
    }
}
