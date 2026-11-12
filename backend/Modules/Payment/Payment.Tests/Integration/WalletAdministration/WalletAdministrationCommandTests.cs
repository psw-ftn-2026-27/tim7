using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletAdministration;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletAdministration;

public class WalletAdministrationCommandTests : BaseIntegrationTest
{
    public WalletAdministrationCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_deposits_to_a_user_with_a_wallet_and_one_transaction_is_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.ExplorerWallet.UserId}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore + 1);
        var stored = assertContext.Wallets.Include(wallet => wallet.Transactions).Single(wallet => wallet.Id == WalletSeed.ExplorerWallet.Id);
        stored.Transactions.Should().ContainSingle()
            .Which.Amount.Should().Be(25);
    }

    [Fact]
    public async Task Deposit_that_is_not_positive_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.ExplorerWallet.UserId}/deposits", new DepositDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore);
    }

    [Fact]
    public async Task Deposit_to_a_user_without_a_wallet_is_rejected_as_not_found_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WellKnownUsers.SecondExplorer}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_deposit()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.ExplorerWallet.UserId}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.ExplorerWallet.UserId}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
