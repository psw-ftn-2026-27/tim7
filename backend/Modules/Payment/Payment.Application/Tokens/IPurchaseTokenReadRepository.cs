namespace Payment.Application.Tokens;

public interface IPurchaseTokenReadRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetEntityIdsByUserAsync(Guid userId);
}
