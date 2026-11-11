using Identity.Core.Connections;
using Shared.Domain;

namespace Identity.Core.Following;

public sealed class FollowingQueries
{
    private readonly IConnectionReadRepository _connectionReadRepository;

    public FollowingQueries(IConnectionReadRepository connectionReadRepository)
    {
        _connectionReadRepository = connectionReadRepository;
    }

    public Task<PageResult<FoundUserDto>> SearchUsersAsync(Guid userId, string email, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _connectionReadRepository.SearchUsersAsync(userId, email, page, pageSize);
    }

    public Task<FollowRequestsDto> GetRequestsAsync(Guid userId)
    {
        return _connectionReadRepository.GetRequestsAsync(userId);
    }

    public Task<PageResult<UserDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _connectionReadRepository.GetFollowersAsync(userId, page, pageSize);
    }

    public Task<PageResult<UserDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _connectionReadRepository.GetFollowingAsync(userId, page, pageSize);
    }
}
