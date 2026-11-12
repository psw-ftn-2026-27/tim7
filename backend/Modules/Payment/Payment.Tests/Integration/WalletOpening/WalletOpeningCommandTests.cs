using System.Net;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletOpening;

public class WalletOpeningCommandTests : BaseIntegrationTest
{
    public WalletOpeningCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_without_a_wallet_opens_a_wallet_with_no_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/wallet", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore + 1);
        var stored = assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.SecondExplorer);
        stored.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task User_with_a_wallet_opens_it_again_and_still_has_exactly_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/wallet", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        assertContext.Wallets.Count(wallet => wallet.UserId == WellKnownUsers.Explorer).Should().Be(1);
    }

    [Fact]
    public async Task Anonymous_user_cannot_open_a_wallet()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/wallet", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
