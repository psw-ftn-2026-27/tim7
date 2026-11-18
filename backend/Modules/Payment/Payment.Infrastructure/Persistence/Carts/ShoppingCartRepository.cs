using Microsoft.EntityFrameworkCore;
using Payment.Application.Carts;
using Payment.Domain.Carts;

namespace Payment.Infrastructure.Persistence.Carts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.ShoppingCarts.FirstOrDefaultAsync(cart => cart.UserId == userId);
    }

    public void Add(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }
}
