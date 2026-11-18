using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed record OrderItem
{
    public Guid EntityId { get; private init; }
    public string Name { get; private init; } = "";
    public int Price { get; private init; }
    public Guid OwnerId { get; private init; }

    private OrderItem() { }

    public OrderItem(Guid entityId, string name, int price, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("An order item requires a name.");
        }
        if (price <= 0)
        {
            throw new DomainException("An order item requires a price in whole EC greater than zero.");
        }

        EntityId = entityId;
        Name = name;
        Price = price;
        OwnerId = ownerId;
    }
}
