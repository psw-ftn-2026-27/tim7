using Payment.Domain.Tokens;

namespace Payment.Application.Tokens;

public interface IPurchaseTokenRepository
{
    Task<List<PurchaseToken>> GetByUserIdAsync(Guid userId);
    void AddRange(IEnumerable<PurchaseToken> tokens);
}
