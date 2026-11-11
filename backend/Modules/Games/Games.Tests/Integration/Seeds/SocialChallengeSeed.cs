using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftEveningWalk = new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, "Hvala što ste se okupili na keju!");
    public static readonly SocialChallenge DraftMorningMeetup = new("Jutarnje druženje", new TimeOnly(7, 0), new TimeOnly(9, 30), 4, 5, 50, "Dobro jutro, vidimo se sutra opet!");
    public static readonly SocialChallenge PublishedNoonPicnic;
    public static readonly SocialChallenge RetiredSunsetGathering;
    public static readonly SocialChallenge ArchivedWinterWalk;

    static SocialChallengeSeed()
    {
        PublishedNoonPicnic = new("Podnevni piknik", new TimeOnly(12, 0), new TimeOnly(14, 0), 8, 6, 75, "Hvala što ste došli na podnevni piknik!");
        PublishedNoonPicnic.Publish();

        RetiredSunsetGathering = new("Okupljanje u sumrak", new TimeOnly(19, 0), new TimeOnly(21, 0), 6, 5, 60, "Hvala što ste došli na okupljanje u sumrak!");
        RetiredSunsetGathering.Publish();
        RetiredSunsetGathering.Retire();

        ArchivedWinterWalk = new("Zimska šetnja", new TimeOnly(10, 0), new TimeOnly(12, 0), 5, 5, 40, "Hvala što ste prošetali zimskim stazama!");
        ArchivedWinterWalk.Publish();
        ArchivedWinterWalk.Archive();
    }

    public static SocialChallenge[] All => [DraftEveningWalk, DraftMorningMeetup, PublishedNoonPicnic, RetiredSunsetGathering, ArchivedWinterWalk];
}
