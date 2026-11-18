using Payment.Domain.Carts;

namespace Payment.Application.Carts;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId);
    void Add(ShoppingCart cart);
}
