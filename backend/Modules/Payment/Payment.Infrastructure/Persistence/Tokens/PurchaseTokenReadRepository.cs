using Microsoft.EntityFrameworkCore;
using Payment.Application.Tokens;

namespace Payment.Infrastructure.Persistence.Tokens;

internal sealed class PurchaseTokenReadRepository : IPurchaseTokenReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseTokenReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid userId, Guid entityId)
    {
        return _dbContext.PurchaseTokens.AnyAsync(token => token.UserId == userId && token.EntityId == entityId);
    }

    public Task<List<Guid>> GetEntityIdsByUserAsync(Guid userId)
    {
        return _dbContext.PurchaseTokens
            .Where(token => token.UserId == userId)
            .Select(token => token.EntityId)
            .ToListAsync();
    }
}
