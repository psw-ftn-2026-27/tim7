using Games.Domain.LocationChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge CityCenterHunt = new(
        "Potraga u centru",
        100,
        new Location(44.8000, 20.4600),
        [
            new HiddenPoint("/images/games/seed-fountain.png", new Location(44.8010, 20.4600)),
            new HiddenPoint("/images/games/seed-bench.png", new Location(44.8020, 20.4600))
        ],
        50);

    public static readonly LocationChallenge RiverbankWalk = new(
        "Šetnja kejom",
        60,
        new Location(44.8200, 20.4400),
        [
            new HiddenPoint("/images/games/seed-pier.png", new Location(44.8200, 20.4415))
        ],
        0);

    public static LocationChallenge[] All => [CityCenterHunt, RiverbankWalk];
}
