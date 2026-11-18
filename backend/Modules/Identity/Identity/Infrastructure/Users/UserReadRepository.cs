using Identity.Contracts;
using Identity.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users;

internal sealed class UserReadRepository : IUserReadRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public UserReadRepository(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserSummary>> GetByIdsAsync(IReadOnlyCollection<Guid> userIds)
    {
        return await _dbContext.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserSummary(user.Id, user.UserName!))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserSummary>> SearchExplorersAsync(string username, int maxResults)
    {
        var normalizedUsername = username.ToUpperInvariant();
        var explorerIds = _dbContext.UserRoles
            .Join(_dbContext.Roles.Where(role => role.Name == "explorer"),
                userRole => userRole.RoleId, role => role.Id, (userRole, role) => userRole.UserId);

        return await _dbContext.Users
            .Where(user => explorerIds.Contains(user.Id)
                && user.NormalizedUserName!.Contains(normalizedUsername))
            .OrderBy(user => user.UserName)
            .Take(maxResults)
            .Select(user => new UserSummary(user.Id, user.UserName!))
            .ToListAsync();
    }
}
