using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
internal static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer@test.com",
        NormalizedUserName = "EXPLORER@TEST.COM",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly ApplicationUser SecondExplorer = new()
    {
        Id = WellKnownUsers.SecondExplorer,
        UserName = "second.explorer@test.com",
        NormalizedUserName = "SECOND.EXPLORER@TEST.COM",
        Email = "second.explorer@test.com",
        NormalizedEmail = "SECOND.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "0b6c1d2e-3f4a-4b5c-8d6e-7f8a9b0c1d2e"
    };

    public static readonly ApplicationUser FollowedExplorer = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0002-000000000001"),
        UserName = "followed.explorer@test.com",
        NormalizedUserName = "FOLLOWED.EXPLORER@TEST.COM",
        Email = "followed.explorer@test.com",
        NormalizedEmail = "FOLLOWED.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "1c7d2e3f-4a5b-4c6d-9e7f-8a9b0c1d2e3f"
    };

    public static readonly ApplicationUser AwaitedExplorer = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0002-000000000002"),
        UserName = "awaited.explorer@test.com",
        NormalizedUserName = "AWAITED.EXPLORER@TEST.COM",
        Email = "awaited.explorer@test.com",
        NormalizedEmail = "AWAITED.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "2d8e3f4a-5b6c-4d7e-8f9a-0b1c2d3e4f5a"
    };

    public static readonly ApplicationUser RequestingExplorer = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0002-000000000003"),
        UserName = "requesting.explorer@test.com",
        NormalizedUserName = "REQUESTING.EXPLORER@TEST.COM",
        Email = "requesting.explorer@test.com",
        NormalizedEmail = "REQUESTING.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "3e9f4a5b-6c7d-4e8f-9a0b-1c2d3e4f5a6b"
    };

    public static readonly ApplicationUser LoneExplorer = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0002-000000000004"),
        UserName = "lone.explorer@test.com",
        NormalizedUserName = "LONE.EXPLORER@TEST.COM",
        Email = "lone.explorer@test.com",
        NormalizedEmail = "LONE.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "4f0a5b6c-7d8e-4f9a-8b1c-2d3e4f5a6b7c"
    };

    public static readonly ApplicationUser Administrator = new()
    {
        Id = WellKnownUsers.Administrator,
        UserName = "admin@test.com",
        NormalizedUserName = "ADMIN@TEST.COM",
        Email = "admin@test.com",
        NormalizedEmail = "ADMIN@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5a1b6c7d-8e9f-4a0b-9c2d-3e4f5a6b7c8d"
    };

    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> SecondExplorerRole = new() { UserId = WellKnownUsers.SecondExplorer, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> FollowedExplorerRole = new() { UserId = FollowedExplorer.Id, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> AwaitedExplorerRole = new() { UserId = AwaitedExplorer.Id, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> RequestingExplorerRole = new() { UserId = RequestingExplorer.Id, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> LoneExplorerRole = new() { UserId = LoneExplorer.Id, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> AdministratorRole = new() { UserId = WellKnownUsers.Administrator, RoleId = RoleSeed.Administrator.Id };

    public static ApplicationUser[] Users =>
        [Explorer, SecondExplorer, FollowedExplorer, AwaitedExplorer, RequestingExplorer, LoneExplorer, Administrator];

    public static object[] All =>
    [
        .. Users,
        ExplorerRole, SecondExplorerRole, FollowedExplorerRole, AwaitedExplorerRole,
        RequestingExplorerRole, LoneExplorerRole, AdministratorRole
    ];
}
