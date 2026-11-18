using Identity.Core.UserPositions;

namespace Identity.Core.PositionSimulation;

public sealed class PositionSimulationQueries
{
    private readonly IUserPositionReadRepository _userPositionReadRepository;

    public PositionSimulationQueries(IUserPositionReadRepository userPositionReadRepository)
    {
        _userPositionReadRepository = userPositionReadRepository;
    }

    public Task<PositionDto?> GetMyPositionAsync(Guid userId)
    {
        return _userPositionReadRepository.GetByUserAsync(userId);
    }
}
