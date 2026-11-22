using Identity.Core.UserPositions;

namespace Identity.Tests.Integration.Seeds;

internal static class UserPositionSeed
{
    public static readonly UserPosition ExplorerStandsInNoviSad;
    public static readonly UserPosition SecondExplorerStandsInBelgrade;

    static UserPositionSeed()
    {
        ExplorerStandsInNoviSad = new(UserSeed.Explorer.Id, new Coordinates(45.2671, 19.8335));

        SecondExplorerStandsInBelgrade = new(UserSeed.SecondExplorer.Id, new Coordinates(44.8125, 20.4612));
    }

    public static UserPosition[] All => [ExplorerStandsInNoviSad, SecondExplorerStandsInBelgrade];
}
