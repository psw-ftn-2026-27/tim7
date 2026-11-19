using Identity.Core.PositionSimulation;
using Identity.Core.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.UserPositions;

internal sealed class UserPositionReadRepository : IUserPositionReadRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public UserPositionReadRepository(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PositionDto?> GetByUserAsync(Guid userId)
    {
        return _dbContext.UserPositions
            .Where(userPosition => userPosition.UserId == userId)
            .Select(userPosition => new PositionDto(userPosition.Coordinates.Latitude, userPosition.Coordinates.Longitude))
            .FirstOrDefaultAsync();
    }
}
