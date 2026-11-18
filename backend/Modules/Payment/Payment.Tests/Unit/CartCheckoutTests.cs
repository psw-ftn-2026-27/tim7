using FluentAssertions;
using Payment.Domain.Carts;
using Payment.Domain.Checkout;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public class CartCheckoutTests
{
    private readonly CartCheckout _checkout = new();
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _tourId = Guid.NewGuid();

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Adding_a_tour_that_is_not_for_sale_or_that_is_already_bought_is_rejected(bool isForSale, bool isBought)
    {
        var cart = new ShoppingCart(_buyerId);
        var boughtEntityIds = isBought ? new List<Guid> { _tourId } : new List<Guid>();

        var add = () => _checkout.AddToCart(cart, _tourId, "Tour", 10, _ownerId, isForSale, boughtEntityIds);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Payment_is_rejected_in_full_when_the_wallet_has_not_enough_EC()
    {
        var cart = CartWithTour(price: 30);
        var buyerWallet = WalletWithBalance(_buyerId, 20);
        var ownerWallet = new Wallet(_ownerId);

        var pay = () => _checkout.Pay(cart, buyerWallet, [ownerWallet], [_tourId], [], DateTime.UtcNow);

        pay.Should().Throw<DomainException>();
        buyerWallet.Balance().Should().Be(20);
        ownerWallet.Balance().Should().Be(0);
        cart.Items.Should().ContainSingle();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Payment_is_rejected_in_full_when_a_tour_was_bought_in_the_meantime_or_is_no_longer_for_sale(bool isBought, bool isForSale)
    {
        var cart = CartWithTour(price: 30);
        var buyerWallet = WalletWithBalance(_buyerId, 100);
        var ownerWallet = new Wallet(_ownerId);
        var boughtEntityIds = isBought ? new List<Guid> { _tourId } : new List<Guid>();
        var forSaleEntityIds = isForSale ? new List<Guid> { _tourId } : new List<Guid>();

        var pay = () => _checkout.Pay(cart, buyerWallet, [ownerWallet], forSaleEntityIds, boughtEntityIds, DateTime.UtcNow);

        pay.Should().Throw<DomainException>();
        buyerWallet.Balance().Should().Be(100);
        ownerWallet.Balance().Should().Be(0);
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void Payment_moves_the_price_to_the_owner_creates_a_token_per_item_and_empties_the_cart()
    {
        var cart = CartWithTour(price: 30);
        var buyerWallet = WalletWithBalance(_buyerId, 100);
        var ownerWallet = new Wallet(_ownerId);

        var tokens = _checkout.Pay(cart, buyerWallet, [ownerWallet], [_tourId], [], DateTime.UtcNow);

        tokens.Should().ContainSingle(token => token.UserId == _buyerId && token.EntityId == _tourId);
        buyerWallet.Balance().Should().Be(70);
        ownerWallet.Balance().Should().Be(30);
        cart.Items.Should().BeEmpty();
    }

    private ShoppingCart CartWithTour(int price)
    {
        var cart = new ShoppingCart(_buyerId);
        cart.AddItem(_tourId, "Tour", price, _ownerId);
        return cart;
    }

    private static Wallet WalletWithBalance(Guid userId, int balance)
    {
        var wallet = new Wallet(userId);
        wallet.Deposit(Guid.NewGuid(), balance);
        return wallet;
    }
}
