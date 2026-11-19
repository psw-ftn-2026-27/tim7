using Games.Domain.LocationChallenges;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeProgressSeed
{
    public static readonly LocationChallengeProgress ExplorerInCityCenterHunt =
        new(WellKnownUsers.Explorer, LocationChallengeSeed.CityCenterHunt.Id);

    public static LocationChallengeProgress[] All => [ExplorerInCityCenterHunt];
}
