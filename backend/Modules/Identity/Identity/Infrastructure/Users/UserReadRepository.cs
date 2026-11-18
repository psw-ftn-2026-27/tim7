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

    public async Task<List<UserSummary>> SearchExplorersAsync(string username, int maxResults)
    {
        var normalizedUsername = username.ToUpperInvariant();

        return await _dbContext.Explorers()
            .Where(user => user.NormalizedUserName!.Contains(normalizedUsername))
            .OrderBy(user => user.UserName)
            .Take(maxResults)
            .Select(user => new UserSummary(user.Id, user.Email!))
            .ToListAsync();
    }
}
