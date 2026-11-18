using Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

public sealed class IdentityApi : IIdentityApi
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityApi(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
}
