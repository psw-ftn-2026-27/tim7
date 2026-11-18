using Shared.Domain;

namespace Payment.Domain.Tokens;

public sealed class PurchaseToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid EntityId { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private PurchaseToken() { }

    public PurchaseToken(Guid userId, Guid entityId, DateTime purchasedAt) : base(Guid.NewGuid())
    {
        UserId = userId;
        EntityId = entityId;
        PurchasedAt = purchasedAt;
    }
}
