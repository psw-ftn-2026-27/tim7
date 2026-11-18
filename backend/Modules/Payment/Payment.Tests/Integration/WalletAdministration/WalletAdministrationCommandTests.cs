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

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.DemoExplorerEmptyWallet.UserId}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore + 1);
        var stored = assertContext.Wallets.Include(wallet => wallet.Transactions).Single(wallet => wallet.Id == WalletSeed.DemoExplorerEmptyWallet.Id);
        stored.Transactions.Should().ContainSingle()
            .Which.Amount.Should().Be(25);
    }

    [Fact]
    public async Task Deposit_that_is_not_positive_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{WalletSeed.DemoExplorerEmptyWallet.UserId}/deposits", new DepositDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore);
    }

    [Fact]
    public async Task Deposit_to_a_user_without_a_wallet_creates_the_wallet_with_that_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{DemoUsers.Explorer3}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Include(wallet => wallet.Transactions).Single(wallet => wallet.UserId == DemoUsers.Explorer3);
        stored.Transactions.Should().ContainSingle()
            .Which.Amount.Should().Be(25);
    }

    [Fact]
    public async Task Deposit_to_a_user_that_does_not_exist_is_rejected_as_not_found_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownUserId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{unknownUserId}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore);
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
    }

    [Fact]
    public async Task Administrator_deposit_to_himself_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(DemoUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Set<Transaction>().Count();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{DemoUsers.Administrator}/deposits", new DepositDto(25));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count().Should().Be(transactionCountBefore);
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
    }

    [Fact]
    public async Task Administrator_reverses_a_deposit_within_24_hours_and_it_is_stored_as_reversed_and_the_balance_is_reduced()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var deposit = WalletSeed.FundedWallet.Transactions[0];

        var response = await client.PostAsync($"/api/payment/wallets/{WalletSeed.FundedWallet.UserId}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Include(wallet => wallet.Transactions).Single(wallet => wallet.Id == WalletSeed.FundedWallet.Id);
        stored.Transactions.Single(transaction => transaction.Id == deposit.Id).IsReversed.Should().BeTrue();
        stored.Balance().Should().Be(0);
    }

    [Fact]
    public async Task Reversing_a_deposit_that_is_already_reversed_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var wallet = WalletSeed.WalletWithReversedAndActiveDeposits;
        var reversedDeposit = wallet.Transactions[0];
        var activeDeposit = wallet.Transactions[1];

        var response = await client.PostAsync($"/api/payment/wallets/{wallet.UserId}/deposits/{reversedDeposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Include(w => w.Transactions).Single(w => w.Id == wallet.Id);
        stored.Transactions.Single(transaction => transaction.Id == reversedDeposit.Id).IsReversed.Should().BeTrue();
        stored.Transactions.Single(transaction => transaction.Id == activeDeposit.Id).IsReversed.Should().BeFalse();
        stored.Balance().Should().Be(20);
    }

    [Fact]
    public async Task Reversing_a_deposit_of_a_user_without_a_wallet_is_rejected_as_not_found_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var deposit = WalletSeed.FundedWallet.Transactions[0];
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var reversedCountBefore = arrangeContext.Set<Transaction>().Count(transaction => transaction.IsReversed);

        var response = await client.PostAsync($"/api/payment/wallets/{WellKnownUsers.SecondExplorer}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Set<Transaction>().Count(transaction => transaction.IsReversed).Should().Be(reversedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reverse_a_deposit()
    {
        var client = Factory.CreateClient();
        var deposit = WalletSeed.FundedWallet.Transactions[0];

        var response = await client.PostAsync($"/api/payment/wallets/{WalletSeed.FundedWallet.UserId}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_reverse_a_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var deposit = WalletSeed.FundedWallet.Transactions[0];

        var response = await client.PostAsync($"/api/payment/wallets/{WalletSeed.FundedWallet.UserId}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
