namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummaryPage> GetUsersAsync(int page, int pageSize);
    Task<IReadOnlyList<UserSummary>> GetUsersByIdsAsync(IReadOnlyCollection<Guid> userIds);
    Task<IReadOnlyList<UserSummary>> SearchExplorersAsync(string username);
    Task<bool> UserExistsAsync(Guid userId);
    Task<UserSummary?> FindUserByEmailAsync(string email);
}
