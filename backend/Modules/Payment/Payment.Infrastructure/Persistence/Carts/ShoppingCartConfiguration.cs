using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Carts;

namespace Payment.Infrastructure.Persistence.Carts;

internal static class ShoppingCartConfiguration
{
    public static void ConfigureShoppingCarts(this ModelBuilder modelBuilder)
    {
        ConfigureShoppingCart(modelBuilder.Entity<ShoppingCart>());
    }

    private static void ConfigureShoppingCart(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.HasIndex(cart => cart.UserId).IsUnique();

        builder.OwnsMany(cart => cart.Items, items =>
        {
            items.ToTable("OrderItems");
            items.WithOwner().HasForeignKey("ShoppingCartId");
            items.Property<int>("Id").ValueGeneratedOnAdd();
            items.HasKey("Id");
            items.Property(item => item.Name).HasMaxLength(200);
        });
    }
}
