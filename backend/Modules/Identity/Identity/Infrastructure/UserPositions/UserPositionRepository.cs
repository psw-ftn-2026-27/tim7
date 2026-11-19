using Identity.Core.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.UserPositions;

internal sealed class UserPositionRepository : IUserPositionRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public UserPositionRepository(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPosition?> GetByUserAsync(Guid userId)
    {
        return _dbContext.UserPositions.FirstOrDefaultAsync(userPosition => userPosition.UserId == userId);
    }

    public void Add(UserPosition userPosition)
    {
        _dbContext.UserPositions.Add(userPosition);
    }
}
