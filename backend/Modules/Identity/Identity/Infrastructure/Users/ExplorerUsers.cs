using Identity.Core;

namespace Identity.Infrastructure.Users;

// The one place that decides who counts as an explorer, shared by every search over users.
internal static class ExplorerUsers
{
    public static IQueryable<ApplicationUser> Explorers(this IdentityModuleDbContext dbContext)
    {
        var explorerIds = dbContext.UserRoles
            .Join(dbContext.Roles.Where(role => role.Name == "explorer"),
                userRole => userRole.RoleId, role => role.Id, (userRole, role) => userRole.UserId);

        return dbContext.Users.Where(user => explorerIds.Contains(user.Id));
    }
}
