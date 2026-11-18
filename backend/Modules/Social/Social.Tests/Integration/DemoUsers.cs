namespace Social.Tests.Integration;

// Users that Identity seeds in development, so IIdentityApi resolves their e-mails.
// WellKnownUsers do not exist in Identity and suit only tests that need no user data.
internal static class DemoUsers
{
    public static readonly Guid Explorer1 = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid Explorer2 = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid Explorer3 = Guid.Parse("20000000-0000-0000-0000-000000000003");
}
