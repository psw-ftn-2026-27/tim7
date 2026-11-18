using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public class WalletTests
{
    [Fact]
    public void Deposit_raises_the_balance_by_its_amount_and_records_the_administrator()
    {
        var wallet = new Wallet(Guid.NewGuid());
        var administratorId = Guid.NewGuid();

        wallet.Deposit(administratorId, 50);

        wallet.Balance().Should().Be(50);
        wallet.Transactions.Should().ContainSingle()
            .Which.AdministratorId.Should().Be(administratorId);
    }

    [Fact]
    public void Deposit_reversed_within_24_hours_is_marked_reversed_and_the_balance_is_reduced_by_its_amount()
    {
        var wallet = WalletWithDeposits(50, 30);
        var deposit = wallet.Transactions[1];

        wallet.ReverseDeposit(deposit.Id, DateTime.UtcNow);

        deposit.IsReversed.Should().BeTrue();
        wallet.Balance().Should().Be(50);
    }

    [Fact]
    public void Deposit_reversed_25_hours_after_it_was_made_is_rejected()
    {
        var wallet = WalletWithDeposits(50);
        var deposit = wallet.Transactions[0];

        var reverse = () => wallet.ReverseDeposit(deposit.Id, deposit.CreatedAt.AddHours(25));

        reverse.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deposit_that_is_already_reversed_cannot_be_reversed_again()
    {
        var wallet = WalletWithDeposits(50, 30);
        var deposit = wallet.Transactions[1];
        wallet.ReverseDeposit(deposit.Id, DateTime.UtcNow);

        var reverseAgain = () => wallet.ReverseDeposit(deposit.Id, DateTime.UtcNow);

        reverseAgain.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deposit_by_the_owner_of_the_wallet_is_rejected()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId);

        var deposit = () => wallet.Deposit(userId, 50);

        deposit.Should().Throw<DomainException>();
    }

    private static Wallet WalletWithDeposits(params int[] amounts)
    {
        var wallet = new Wallet(Guid.NewGuid());
        foreach (var amount in amounts)
        {
            wallet.Deposit(Guid.NewGuid(), amount);
        }

        return wallet;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deposit_that_is_not_positive_is_rejected(int amount)
    {
        var wallet = new Wallet(Guid.NewGuid());

        var deposit = () => wallet.Deposit(Guid.NewGuid(), amount);

        deposit.Should().Throw<DomainException>();
    }
}
