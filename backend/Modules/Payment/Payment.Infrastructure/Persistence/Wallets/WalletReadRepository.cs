using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletAdministration;
using Payment.Application.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<UserDepositDto>> GetDepositsByUserAsync(Guid userId)
    {
        return _dbContext.Wallets
            .Where(wallet => wallet.UserId == userId)
            .SelectMany(wallet => wallet.Transactions)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Select(transaction => new UserDepositDto(
                transaction.Id,
                transaction.Amount,
                transaction.CreatedAt,
                transaction.AdministratorId,
                transaction.IsReversed))
            .ToListAsync();
    }
}
