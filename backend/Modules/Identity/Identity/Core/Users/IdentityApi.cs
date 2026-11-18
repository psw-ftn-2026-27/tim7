using Identity.Contracts;

namespace Identity.Core.Users;

public sealed class IdentityApi : IIdentityApi
{
    private const int MaxSearchResults = 10;

    private readonly IUserReadRepository _userReadRepository;

    public IdentityApi(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
    }

    public Task<IReadOnlyList<UserSummary>> GetUsersAsync(IReadOnlyCollection<Guid> userIds)
    {
        return _userReadRepository.GetByIdsAsync(userIds);
    }

    public Task<IReadOnlyList<UserSummary>> SearchExplorersAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult<IReadOnlyList<UserSummary>>([]);
        }

        return _userReadRepository.SearchExplorersAsync(username.Trim(), MaxSearchResults);
    }
}
