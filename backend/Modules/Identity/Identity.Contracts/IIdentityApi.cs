namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummaryPage> GetUsersAsync(int page, int pageSize);
    Task<bool> UserExistsAsync(Guid userId);
}
