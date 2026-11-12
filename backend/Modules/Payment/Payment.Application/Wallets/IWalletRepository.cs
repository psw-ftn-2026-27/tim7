using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    Task<List<Wallet>> GetPageAsync(int page, int pageSize);
    Task<int> CountAsync();
    void Add(Wallet wallet);
}
