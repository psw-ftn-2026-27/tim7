using Identity.Contracts;

namespace Identity.Core.Users;

public interface IUserReadRepository
{
    Task<List<UserSummary>> SearchExplorersAsync(string username, int maxResults);
}
