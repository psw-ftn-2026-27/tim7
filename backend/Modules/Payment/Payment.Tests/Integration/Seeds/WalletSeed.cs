using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorerWallet = new(WellKnownUsers.Explorer);

    public static readonly Wallet FundedWallet = new(Guid.NewGuid());

    static WalletSeed()
    {
        FundedWallet.Deposit(WellKnownUsers.Administrator, 50);
    }

    public static Wallet[] All => [ExplorerWallet, FundedWallet];
}
