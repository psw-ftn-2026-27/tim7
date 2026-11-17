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
    public async Task Administrator_sees_the_reversed_and_the_non_reversed_deposits_of_a_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var wallet = WalletSeed.WalletWithReversedAndActiveDeposits;
        var reversedDeposit = wallet.Transactions[0];
        var activeDeposit = wallet.Transactions[1];

        var deposits = await client.GetFromJsonAsync<List<UserDepositDto>>($"/api/payment/wallets/{wallet.UserId}/deposits", JsonOptions);

        deposits.Should().Contain(deposit => deposit.Id == reversedDeposit.Id && deposit.Amount == 30 && deposit.IsReversed);
        deposits.Should().Contain(deposit => deposit.Id == activeDeposit.Id && deposit.Amount == 20 && !deposit.IsReversed);
        deposits.Should().OnlyContain(deposit => deposit.AdministratorId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_the_deposits_of_a_user()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/payment/wallets/{WalletSeed.FundedWallet.UserId}/deposits");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_read_the_deposits_of_a_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/wallets/{WalletSeed.FundedWallet.UserId}/deposits");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
