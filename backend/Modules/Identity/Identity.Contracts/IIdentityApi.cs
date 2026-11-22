namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummaryPage> GetUsersAsync(int page, int pageSize);
    Task<bool> UserExistsAsync(Guid userId);
    Task<UserSummary?> FindUserByEmailAsync(string email);
    Task<List<UserSummary>> GetUsersByIdsAsync(List<Guid> userIds);
    Task<List<UserSummary>> SearchExplorersAsync(string username);
    // The users this user follows, that is, whose follow request they sent and the other accepted.
    Task<List<UserSummary>> GetFollowedUsersAsync(Guid userId);
}
