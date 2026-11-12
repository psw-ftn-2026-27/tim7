using Microsoft.EntityFrameworkCore;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wallet?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.UserId == userId);
    }

    public Task<List<Wallet>> GetPageAsync(int page, int pageSize)
    {
        return _dbContext.Wallets
            .Include(wallet => wallet.Transactions)
            .OrderBy(wallet => wallet.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<int> CountAsync()
    {
        return _dbContext.Wallets.CountAsync();
    }

    public void Add(Wallet wallet)
    {
        _dbContext.Wallets.Add(wallet);
    }
}
