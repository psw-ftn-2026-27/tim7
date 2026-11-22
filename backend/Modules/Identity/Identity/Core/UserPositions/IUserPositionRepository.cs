namespace Identity.Core.UserPositions;

public interface IUserPositionRepository
{
    Task<UserPosition?> GetByUserAsync(Guid userId);
    void Add(UserPosition userPosition);
}
