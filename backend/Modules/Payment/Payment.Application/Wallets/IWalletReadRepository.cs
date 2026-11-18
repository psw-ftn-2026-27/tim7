using Payment.Application.WalletAdministration;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<List<UserDepositDto>> GetDepositsByUserAsync(Guid userId);
}
