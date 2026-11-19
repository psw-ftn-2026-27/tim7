using Payment.Application.Tokens;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly IPurchaseTokenReadRepository _tokenReadRepository;

    public PaymentApi(IPurchaseTokenReadRepository tokenReadRepository)
    {
        _tokenReadRepository = tokenReadRepository;
    }

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId)
    {
        return _tokenReadRepository.ExistsAsync(userId, entityId);
    }

    public Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId)
    {
        return _tokenReadRepository.GetEntityIdsByUserAsync(userId);
    }
}
