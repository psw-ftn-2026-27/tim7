using Identity.Core.PositionSimulation;

namespace Identity.Core.UserPositions;

public interface IUserPositionReadRepository
{
    Task<PositionDto?> GetByUserAsync(Guid userId);
}
