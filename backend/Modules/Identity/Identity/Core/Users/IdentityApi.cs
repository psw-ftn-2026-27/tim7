using Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Users;

public sealed class IdentityApi : IIdentityApi
{
    private const int MaxSearchResults = 10;

    private readonly IUserReadRepository _userReadRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityApi(IUserReadRepository userReadRepository, UserManager<ApplicationUser> userManager)
    {
        _userReadRepository = userReadRepository;
        _userManager = userManager;
    }

    public async Task<UserSummaryPage> GetUsersAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _userManager.Users.CountAsync();
        var items = await _userManager.Users
            .OrderBy(user => user.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserSummary(user.Id, user.UserName!))
            .ToListAsync();

        return new UserSummaryPage(items, totalCount);
    }

    public Task<IReadOnlyList<UserSummary>> GetUsersByIdsAsync(IReadOnlyCollection<Guid> userIds)
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

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) is not null;
    }

    public async Task<UserSummary?> FindUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserSummary(user.Id, user.UserName!);
    }
}
