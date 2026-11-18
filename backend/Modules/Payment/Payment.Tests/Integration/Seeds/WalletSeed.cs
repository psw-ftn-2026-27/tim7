using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorerWallet = new(WellKnownUsers.Explorer);

    public static readonly Wallet FundedWallet = new(DemoUsers.Explorer2);

    public static readonly Wallet DemoExplorerEmptyWallet = new(DemoUsers.Explorer1);

    public static readonly Wallet WalletWithReversedAndActiveDeposits = new(Guid.NewGuid());

    static WalletSeed()
    {
        FundedWallet.Deposit(WellKnownUsers.Administrator, 50);

        WalletWithReversedAndActiveDeposits.Deposit(WellKnownUsers.Administrator, 30);
        WalletWithReversedAndActiveDeposits.Deposit(WellKnownUsers.Administrator, 20);
        WalletWithReversedAndActiveDeposits.ReverseDeposit(WalletWithReversedAndActiveDeposits.Transactions[0].Id, DateTime.UtcNow);
    }

    public static Wallet[] All => [ExplorerWallet, FundedWallet, DemoExplorerEmptyWallet, WalletWithReversedAndActiveDeposits];
}
