using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    Task<List<Wallet>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds);
    void Add(Wallet wallet);
}
