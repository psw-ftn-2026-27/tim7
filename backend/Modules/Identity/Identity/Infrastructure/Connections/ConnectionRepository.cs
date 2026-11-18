using Identity.Core.Connections;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Connections;

internal sealed class ConnectionRepository : IConnectionRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public ConnectionRepository(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Connection?> GetByUsersAsync(Guid userId, Guid otherUserId)
    {
        return _dbContext.Connections.FirstOrDefaultAsync(connection =>
            (connection.UserAId == userId && connection.UserBId == otherUserId)
            || (connection.UserAId == otherUserId && connection.UserBId == userId));
    }

    public void Add(Connection connection)
    {
        _dbContext.Connections.Add(connection);
    }
}
