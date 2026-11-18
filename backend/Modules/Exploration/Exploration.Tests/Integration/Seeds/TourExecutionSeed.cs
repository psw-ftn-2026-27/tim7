using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    // A third tourist, local to these seeds: Explorer and SecondExplorer are already bound to other tests.
    public static readonly Guid ThirdExplorer = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public static readonly TourExecution ExplorersActiveExecution = new(WellKnownUsers.Explorer, TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint.Id, new Location(45.2690, 19.8790));
    public static readonly TourExecution SecondExplorersAbandonedExecution;
    public static readonly TourExecution ThirdExplorersActiveExecutionOfArchivedTour = new(ThirdExplorer, TourSeed.ArchivedLakeside.Id, new Location(46.0980, 19.7690));

    static TourExecutionSeed()
    {
        SecondExplorersAbandonedExecution = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedOldTownWalk.Id, new Location(45.2549, 19.8449));
        SecondExplorersAbandonedExecution.Abandon();
    }

    public static TourExecution[] All => [ExplorersActiveExecution, SecondExplorersAbandonedExecution, ThirdExplorersActiveExecutionOfArchivedTour];
}
