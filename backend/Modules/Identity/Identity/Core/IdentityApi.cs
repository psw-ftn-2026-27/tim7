using Identity.Contracts;
using Identity.Core.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

public sealed class IdentityApi : IIdentityApi
{
    private const int MaxSearchResults = 10;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserReadRepository _userReadRepository;

    public IdentityApi(UserManager<ApplicationUser> userManager, IUserReadRepository userReadRepository)
    {
        _userManager = userManager;
        _userReadRepository = userReadRepository;
    }

    public async Task<UserSummaryPage> GetUsersAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _userManager.Users.CountAsync();
        var items = await _userManager.Users
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserSummary(user.Id, user.Email!))
            .ToListAsync();

        return new UserSummaryPage(items, totalCount);
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) is not null;
    }

    public async Task<UserSummary?> FindUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserSummary(user.Id, user.Email!);
    }

    public async Task<List<UserSummary>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        return await _userManager.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserSummary(user.Id, user.Email!))
            .ToListAsync();
    }

    // Searching by role needs the role tables, which UserManager does not query, so it goes through the read repository.
    public async Task<List<UserSummary>> SearchExplorersAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return [];
        }

        return await _userReadRepository.SearchExplorersAsync(username.Trim(), MaxSearchResults);
    }
}
