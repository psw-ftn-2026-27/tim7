using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public Guid UserId { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    private ShoppingCart() { }

    public ShoppingCart(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public void AddItem(Guid entityId, string name, int price, Guid ownerId)
    {
        if (_items.Any(item => item.EntityId == entityId))
        {
            throw new DomainException("The item is already in the cart.");
        }
        if (ownerId == UserId)
        {
            throw new DomainException("You cannot buy your own item.");
        }

        _items.Add(new OrderItem(entityId, name, price, ownerId));
    }

    public void RemoveItem(Guid entityId)
    {
        var item = _items.FirstOrDefault(item => item.EntityId == entityId);
        if (item is null)
        {
            throw new DomainException("The item is not in the cart.");
        }

        _items.Remove(item);
    }

    public int TotalPrice()
    {
        return _items.Sum(item => item.Price);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
