using Identity.Contracts;

namespace Identity.Core.Users;

public interface IUserReadRepository
{
    Task<IReadOnlyList<UserSummary>> GetByIdsAsync(IReadOnlyCollection<Guid> userIds);
    Task<IReadOnlyList<UserSummary>> SearchExplorersAsync(string username, int maxResults);
}
