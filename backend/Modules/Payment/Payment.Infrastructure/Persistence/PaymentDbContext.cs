using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.Carts;
using Payment.Domain.Tokens;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Carts;
using Payment.Infrastructure.Persistence.Tokens;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<PurchaseToken> PurchaseTokens => Set<PurchaseToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureShoppingCarts();
        builder.ConfigurePurchaseTokens();
    }
}
