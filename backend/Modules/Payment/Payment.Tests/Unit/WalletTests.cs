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
