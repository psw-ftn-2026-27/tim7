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
        var found = FindOtherExplorers(userId, email);

        var totalCount = await found.CountAsync();
        var items = await WithRelationship(found, userId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<FoundUserDto>(items, totalCount);
    }

    private IQueryable<ApplicationUser> FindOtherExplorers(Guid userId, string email)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var explorerIds = _dbContext.UserRoles
            .Join(_dbContext.Roles.Where(role => role.Name == "explorer"),
                userRole => userRole.RoleId, role => role.Id, (userRole, role) => userRole.UserId);

        return _dbContext.Users
            .Where(user => user.Id != userId
                && explorerIds.Contains(user.Id)
                && user.NormalizedEmail!.Contains(normalizedEmail))
            .OrderBy(user => user.Email);
    }

    private IQueryable<FoundUserDto> WithRelationship(IQueryable<ApplicationUser> users, Guid userId)
    {
        var sentRequests = _dbContext.Set<FollowRequest>().Where(request => request.SenderId == userId);

        return users.Select(user => new FoundUserDto(
            user.Id,
            user.Email!,
            sentRequests.Any(request => request.RecipientId == user.Id && request.Status == FollowRequestStatus.Accepted)
                ? RelationshipStatus.Following
                : sentRequests.Any(request => request.RecipientId == user.Id && request.Status == FollowRequestStatus.Pending)
                    ? RelationshipStatus.Pending
                    : RelationshipStatus.None));
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
