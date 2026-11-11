using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public int Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid AdministratorId { get; private set; }
    public bool IsReversed { get; private set; }

    private Transaction() { }
}
