namespace Payment.Contracts;

public interface IPaymentApi
{
    Task<bool> HasPurchasedAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId);
}
