using Identity.Core.Following;
using Shared.Domain;

namespace Identity.Core.Connections;

public interface IConnectionReadRepository
{
    Task<PageResult<FoundUserDto>> SearchUsersAsync(Guid userId, string email, int page, int pageSize);
    Task<FollowRequestsDto> GetRequestsAsync(Guid userId);
    Task<PageResult<UserDto>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<PageResult<UserDto>> GetFollowingAsync(Guid userId, int page, int pageSize);
}
