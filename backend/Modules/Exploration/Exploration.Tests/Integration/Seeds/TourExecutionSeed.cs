using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    // Tourists local to these seeds: Explorer and SecondExplorer are already bound to other tests.
    public static readonly Guid ThirdExplorer = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid FourthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid FifthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000006");

    public static readonly TourExecution ExplorersActiveExecution = new(WellKnownUsers.Explorer, TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint.Id, new Location(45.2690, 19.8790));
    public static readonly TourExecution SecondExplorersAbandonedExecution;
    public static readonly TourExecution ThirdExplorersActiveExecutionOfArchivedTour = new(ThirdExplorer, TourSeed.ArchivedLakeside.Id, new Location(46.0980, 19.7690));
    public static readonly TourExecution FourthExplorersActiveExecutionPastTheFirstKeyPoint;
    public static readonly TourExecution FifthExplorersAbandonedExecutionPastTheFirstKeyPoint;
    public static readonly TourExecution FifthExplorersCompletedExecution;

    static TourExecutionSeed()
    {
        SecondExplorersAbandonedExecution = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedOldTownWalk.Id, new Location(45.2549, 19.8449));
        SecondExplorersAbandonedExecution.Abandon();

        // Built before the fifth tourist's sessions, so its completion of the first key point is the earliest one of the tour.
        FourthExplorersActiveExecutionPastTheFirstKeyPoint = new(FourthExplorer, TourSeed.PublishedVineyards.Id, new Location(45.2030, 19.9345));
        FourthExplorersActiveExecutionPastTheFirstKeyPoint.RecordPosition(new Location(45.2024, 19.9338));
        FourthExplorersActiveExecutionPastTheFirstKeyPoint.CompleteKeyPoint(TourSeed.PublishedVineyards.KeyPoints[0].Id, 2);

        FifthExplorersAbandonedExecutionPastTheFirstKeyPoint = new(FifthExplorer, TourSeed.PublishedVineyards.Id, new Location(45.2030, 19.9345));
        FifthExplorersAbandonedExecutionPastTheFirstKeyPoint.RecordPosition(new Location(45.2025, 19.9339));
        FifthExplorersAbandonedExecutionPastTheFirstKeyPoint.CompleteKeyPoint(TourSeed.PublishedVineyards.KeyPoints[0].Id, 2);
        FifthExplorersAbandonedExecutionPastTheFirstKeyPoint.Abandon();

        FifthExplorersCompletedExecution = new(FifthExplorer, TourSeed.PublishedVineyards.Id, new Location(45.2030, 19.9345));
        FifthExplorersCompletedExecution.RecordPosition(new Location(45.2025, 19.9339));
        FifthExplorersCompletedExecution.CompleteKeyPoint(TourSeed.PublishedVineyards.KeyPoints[0].Id, 2);
        FifthExplorersCompletedExecution.RecordPosition(new Location(45.1960, 19.9200));
        FifthExplorersCompletedExecution.CompleteKeyPoint(TourSeed.PublishedVineyards.KeyPoints[1].Id, 2);
    }

    public static TourExecution[] All =>
    [
        ExplorersActiveExecution,
        SecondExplorersAbandonedExecution,
        ThirdExplorersActiveExecutionOfArchivedTour,
        FourthExplorersActiveExecutionPastTheFirstKeyPoint,
        FifthExplorersAbandonedExecutionPastTheFirstKeyPoint,
        FifthExplorersCompletedExecution
    ];
}
