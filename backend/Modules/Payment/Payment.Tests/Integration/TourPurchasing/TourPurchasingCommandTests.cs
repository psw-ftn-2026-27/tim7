using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.TourPurchasing;

public class TourPurchasingCommandTests : BaseIntegrationTest
{
    public TourPurchasingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_tour_to_the_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/payment/cart/items/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_tour_to_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/cart/items/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_tour_from_the_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/payment/cart/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_tour_from_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/payment/cart/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_pay_the_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/cart/payment", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_pay_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/cart/payment", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Payment_is_rejected_in_full_when_a_tour_in_the_cart_is_no_longer_for_sale()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer2, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/cart/payment", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.PurchaseTokens.Count().Should().Be(tokenCountBefore);
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        var buyerWallet = assertContext.Wallets.Include(wallet => wallet.Transactions).Single(wallet => wallet.Id == WalletSeed.FundedWallet.Id);
        buyerWallet.Balance().Should().Be(50);
        var cart = assertContext.ShoppingCarts.Include(cart => cart.Items).Single(cart => cart.Id == ShoppingCartSeed.CartWithTourUnknownToExploration.Id);
        cart.Items.Should().ContainSingle();
    }
}
