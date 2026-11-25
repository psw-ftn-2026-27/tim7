using Games.Domain.SocialChallenges;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeRunSeed
{
    public static readonly SocialChallengeRun SecondExplorerWaitingAtAllDayGathering;
    public static readonly SocialChallengeRun ExplorerAndSecondExplorerInRangeAtAllDayMeetup;
    public static readonly SocialChallengeRun ExplorerAndSecondExplorerSolvedAllDayConcert;

    static SocialChallengeRunSeed()
    {
        var gathering = SocialChallengeSeed.PublishedAllDayGathering;
        var meetup = SocialChallengeSeed.PublishedAllDayMeetup;
        var concert = SocialChallengeSeed.PublishedAllDayConcert;

        SecondExplorerWaitingAtAllDayGathering = new SocialChallengeRun(gathering.Id, WellKnownUsers.SecondExplorer);

        // The second explorer's check is dated a day ahead, so it stays fresh however long the test run takes.
        ExplorerAndSecondExplorerInRangeAtAllDayMeetup = new SocialChallengeRun(meetup.Id, WellKnownUsers.SecondExplorer);
        ExplorerAndSecondExplorerInRangeAtAllDayMeetup.Join(WellKnownUsers.Explorer, false);
        ExplorerAndSecondExplorerInRangeAtAllDayMeetup.ReportPosition(
            WellKnownUsers.SecondExplorer, true, DateTime.UtcNow.AddDays(1), meetup.Gathering.People, meetup.Xp);

        ExplorerAndSecondExplorerSolvedAllDayConcert = new SocialChallengeRun(concert.Id, WellKnownUsers.Explorer);
        ExplorerAndSecondExplorerSolvedAllDayConcert.Join(WellKnownUsers.SecondExplorer, false);
        ExplorerAndSecondExplorerSolvedAllDayConcert.ReportPosition(
            WellKnownUsers.Explorer, true, DateTime.UtcNow, concert.Gathering.People, concert.Xp);
        ExplorerAndSecondExplorerSolvedAllDayConcert.ReportPosition(
            WellKnownUsers.SecondExplorer, true, DateTime.UtcNow, concert.Gathering.People, concert.Xp);
    }

    public static SocialChallengeRun[] All =>
    [
        SecondExplorerWaitingAtAllDayGathering,
        ExplorerAndSecondExplorerInRangeAtAllDayMeetup,
        ExplorerAndSecondExplorerSolvedAllDayConcert
    ];
}
