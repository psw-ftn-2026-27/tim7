using Microsoft.EntityFrameworkCore;
using Payment.Application.Tokens;
using Payment.Domain.Tokens;

namespace Payment.Infrastructure.Persistence.Tokens;

internal sealed class PurchaseTokenRepository : IPurchaseTokenRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseTokenRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PurchaseToken>> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.PurchaseTokens.Where(token => token.UserId == userId).ToListAsync();
    }

    public void AddRange(IEnumerable<PurchaseToken> tokens)
    {
        _dbContext.PurchaseTokens.AddRange(tokens);
    }
}
