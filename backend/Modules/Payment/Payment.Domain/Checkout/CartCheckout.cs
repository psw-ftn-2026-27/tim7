using Payment.Domain.Carts;
using Payment.Domain.Tokens;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Checkout;

public sealed class CartCheckout
{
    // The price is nullable because an entity that is not for sale (free or a draft) has none.
    public void AddToCart(ShoppingCart cart, Guid entityId, string name, int? price, Guid ownerId, bool isForSale, IReadOnlyCollection<Guid> boughtEntityIds)
    {
        if (!isForSale)
        {
            throw new DomainException("The item is not for sale.");
        }
        if (boughtEntityIds.Contains(entityId))
        {
            throw new DomainException("You have already bought this item.");
        }

        cart.AddItem(entityId, name, price.GetValueOrDefault(), ownerId);
    }

    // Everything is checked before anything changes, so a rejected payment leaves wallets, tokens and the cart as they were.
    public List<PurchaseToken> Pay(
        ShoppingCart cart,
        Wallet buyerWallet,
        IReadOnlyCollection<Wallet> ownerWallets,
        IReadOnlyCollection<Guid> forSaleEntityIds,
        IReadOnlyCollection<Guid> boughtEntityIds,
        DateTime now)
    {
        if (cart.Items.Count == 0)
        {
            throw new DomainException("The cart is empty.");
        }
        if (cart.Items.Any(item => boughtEntityIds.Contains(item.EntityId)))
        {
            throw new DomainException("An item in the cart has already been bought.");
        }
        if (cart.Items.Any(item => !forSaleEntityIds.Contains(item.EntityId)))
        {
            throw new DomainException("An item in the cart is no longer for sale.");
        }
        if (cart.Items.Any(item => ownerWallets.All(wallet => wallet.UserId != item.OwnerId)))
        {
            throw new DomainException("The owner of an item has no wallet.");
        }
        if (cart.TotalPrice() > buyerWallet.Balance())
        {
            throw new DomainException("There is not enough EC in the wallet.");
        }

        var tokens = new List<PurchaseToken>();
        foreach (var item in cart.Items)
        {
            buyerWallet.Spend(item.EntityId, item.Price, now);
            ownerWallets.First(wallet => wallet.UserId == item.OwnerId).Earn(item.EntityId, item.Price, now);
            tokens.Add(new PurchaseToken(cart.UserId, item.EntityId, now));
        }

        cart.Clear();
        return tokens;
    }
}
