using Identity.Core;
using Identity.Core.Connections;
using Identity.Core.Following;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Identity.Infrastructure.Connections;

internal sealed class ConnectionReadRepository : IConnectionReadRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public ConnectionReadRepository(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<FoundUserDto>> SearchUsersAsync(Guid userId, string email, int page, int pageSize)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var requests = _dbContext.Set<FollowRequest>();
        var found = _dbContext.Users
            .Where(user => user.Id != userId && user.NormalizedEmail!.Contains(normalizedEmail))
            .OrderBy(user => user.Email);

        var totalCount = await found.CountAsync();
        var items = await found
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new FoundUserDto(
                user.Id,
                user.Email!,
                requests.Any(request => request.SenderId == userId
                    && request.RecipientId == user.Id
                    && request.Status == FollowRequestStatus.Accepted)
                    ? RelationshipStatus.Following
                    : requests.Any(request => request.SenderId == userId
                        && request.RecipientId == user.Id
                        && request.Status == FollowRequestStatus.Pending)
                        ? RelationshipStatus.Pending
                        : RelationshipStatus.None))
            .ToListAsync();

        return new PageResult<FoundUserDto>(items, totalCount);
    }

    public async Task<FollowRequestsDto> GetRequestsAsync(Guid userId)
    {
        var requests = _dbContext.Set<FollowRequest>();

        var received = await requests
            .Where(request => request.RecipientId == userId && request.Status == FollowRequestStatus.Pending)
            .Join(_dbContext.Users, request => request.SenderId, user => user.Id,
                (request, user) => new { request.Status, user.Id, user.Email })
            .OrderBy(row => row.Email)
            .Select(row => new FollowRequestDto(row.Id, row.Email!, row.Status))
            .ToListAsync();

        var sent = await requests
            .Where(request => request.SenderId == userId)
            .Join(_dbContext.Users, request => request.RecipientId, user => user.Id,
                (request, user) => new { request.Status, user.Id, user.Email })
            .OrderBy(row => row.Email)
            .Select(row => new FollowRequestDto(row.Id, row.Email!, row.Status))
            .ToListAsync();

        return new FollowRequestsDto(received, sent);
    }

    public Task<PageResult<UserDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        var requests = _dbContext.Set<FollowRequest>();
        var followers = _dbContext.Users.Where(user => requests.Any(request => request.SenderId == user.Id
            && request.RecipientId == userId
            && request.Status == FollowRequestStatus.Accepted));

        return ToPageAsync(followers, page, pageSize);
    }

    public Task<PageResult<UserDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        var requests = _dbContext.Set<FollowRequest>();
        var followed = _dbContext.Users.Where(user => requests.Any(request => request.SenderId == userId
            && request.RecipientId == user.Id
            && request.Status == FollowRequestStatus.Accepted));

        return ToPageAsync(followed, page, pageSize);
    }

    private static async Task<PageResult<UserDto>> ToPageAsync(IQueryable<ApplicationUser> users, int page, int pageSize)
    {
        var totalCount = await users.CountAsync();
        var items = await users
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserDto(user.Id, user.Email!))
            .ToListAsync();

        return new PageResult<UserDto>(items, totalCount);
    }
}
