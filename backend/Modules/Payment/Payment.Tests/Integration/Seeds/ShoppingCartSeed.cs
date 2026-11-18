using Payment.Domain.Carts;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly ShoppingCart CartWithTourUnknownToExploration = new(DemoUsers.Explorer2);

    static ShoppingCartSeed()
    {
        CartWithTourUnknownToExploration.AddItem(Guid.NewGuid(), "Tour unknown to Exploration", 30, Guid.NewGuid());
    }

    public static ShoppingCart[] All => [CartWithTourUnknownToExploration];
}
