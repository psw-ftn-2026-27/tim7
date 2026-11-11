using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletBrowsing;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletBrowsing;

public class WalletBrowsingQueryTests : BaseIntegrationTest
{
    public WalletBrowsingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_with_a_wallet_sees_a_zero_balance_and_no_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallet", JsonOptions);

        wallet!.Balance.Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_a_wallet()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallet");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
