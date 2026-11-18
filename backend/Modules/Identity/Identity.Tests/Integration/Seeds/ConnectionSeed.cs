using Identity.Core.Connections;

namespace Identity.Tests.Integration.Seeds;

internal static class ConnectionSeed
{
    public static readonly Connection SecondExplorerFollowsExplorer;
    public static readonly Connection ExplorerFollowsFollowedExplorer;
    public static readonly Connection ExplorerAwaitsAwaitedExplorer;
    public static readonly Connection RequestingExplorerAwaitsExplorer;

    static ConnectionSeed()
    {
        SecondExplorerFollowsExplorer = new(UserSeed.SecondExplorer.Id, UserSeed.Explorer.Id);
        SecondExplorerFollowsExplorer.SendRequest(UserSeed.SecondExplorer.Id);
        SecondExplorerFollowsExplorer.Accept(UserSeed.Explorer.Id);

        ExplorerFollowsFollowedExplorer = new(UserSeed.Explorer.Id, UserSeed.FollowedExplorer.Id);
        ExplorerFollowsFollowedExplorer.SendRequest(UserSeed.Explorer.Id);
        ExplorerFollowsFollowedExplorer.Accept(UserSeed.FollowedExplorer.Id);

        ExplorerAwaitsAwaitedExplorer = new(UserSeed.Explorer.Id, UserSeed.AwaitedExplorer.Id);
        ExplorerAwaitsAwaitedExplorer.SendRequest(UserSeed.Explorer.Id);

        RequestingExplorerAwaitsExplorer = new(UserSeed.RequestingExplorer.Id, UserSeed.Explorer.Id);
        RequestingExplorerAwaitsExplorer.SendRequest(UserSeed.RequestingExplorer.Id);
    }

    public static Connection[] All =>
    [
        SecondExplorerFollowsExplorer, ExplorerFollowsFollowedExplorer, ExplorerAwaitsAwaitedExplorer, RequestingExplorerAwaitsExplorer
    ];
}
