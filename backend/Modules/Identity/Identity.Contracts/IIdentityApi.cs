namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(IReadOnlyCollection<Guid> userIds);
    Task<IReadOnlyList<UserSummary>> SearchExplorersAsync(string username);
}
