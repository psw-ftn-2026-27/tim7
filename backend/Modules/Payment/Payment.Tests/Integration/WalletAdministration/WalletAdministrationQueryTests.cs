using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletAdministration;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletAdministration;

public class WalletAdministrationQueryTests : BaseIntegrationTest
{
    public WalletAdministrationQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_users_with_an_empty_wallet_and_with_a_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var page = await client.GetFromJsonAsync<PageResult<UserBalanceDto>>("/api/payment/wallets?page=1&pageSize=100", JsonOptions);

        page!.Items.Should().Contain(user => user.UserId == WalletSeed.ExplorerWallet.UserId && user.Balance == 0);
        page.Items.Should().Contain(user => user.UserId == WalletSeed.FundedWallet.UserId && user.Balance == 50);
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_users_with_balances()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_read_users_with_balances()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync("/api/payment/wallets");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
