using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftEveningWalk = new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, "Hvala što ste se okupili na keju!", new Location(45.2510, 19.8550));
    public static readonly SocialChallenge DraftMorningMeetup = new("Jutarnje druženje", new TimeOnly(7, 0), new TimeOnly(9, 30), 4, 5, 50, "Dobro jutro, vidimo se sutra opet!", null);
    public static readonly SocialChallenge PublishedNoonPicnic;
    public static readonly SocialChallenge RetiredSunsetGathering;
    public static readonly SocialChallenge ArchivedWinterWalk;
    // The all-day challenges are active at any time a test runs.
    public static readonly SocialChallenge PublishedAllDayStroll;
    public static readonly SocialChallenge PublishedAllDayGathering;
    public static readonly SocialChallenge PublishedAllDayMeetup;
    public static readonly SocialChallenge PublishedAllDayConcert;

    static SocialChallengeSeed()
    {
        PublishedAllDayStroll = new("Celodnevna šetnja", new TimeOnly(0, 0), new TimeOnly(23, 59, 59), 2, 10, 80, "Hvala što ste prošetali sa nama!", new Location(45.2600, 19.8300));
        PublishedAllDayStroll.Publish();

        PublishedAllDayGathering = new("Celodnevno okupljanje", new TimeOnly(0, 0), new TimeOnly(23, 59, 59), 3, 10, 90, "Hvala što ste se okupili kod fontane!", new Location(45.2610, 19.8310));
        PublishedAllDayGathering.Publish();

        PublishedAllDayMeetup = new("Celodnevni susret", new TimeOnly(0, 0), new TimeOnly(23, 59, 59), 2, 10, 70, "Hvala što ste došli na susret kod mosta!", new Location(45.2620, 19.8320));
        PublishedAllDayMeetup.Publish();

        PublishedAllDayConcert = new("Celodnevni koncert", new TimeOnly(0, 0), new TimeOnly(23, 59, 59), 2, 10, 120, "Hvala što ste zajedno slušali koncert!", new Location(45.2630, 19.8330));
        PublishedAllDayConcert.Publish();

        PublishedNoonPicnic = new("Podnevni piknik", new TimeOnly(12, 0), new TimeOnly(14, 0), 8, 6, 75, "Hvala što ste došli na podnevni piknik!", new Location(45.2470, 19.8520));
        PublishedNoonPicnic.Publish();

        RetiredSunsetGathering = new("Okupljanje u sumrak", new TimeOnly(19, 0), new TimeOnly(21, 0), 6, 5, 60, "Hvala što ste došli na okupljanje u sumrak!", new Location(45.2532, 19.8614));
        RetiredSunsetGathering.Publish();
        RetiredSunsetGathering.Retire();

        ArchivedWinterWalk = new("Zimska šetnja", new TimeOnly(10, 0), new TimeOnly(12, 0), 5, 5, 40, "Hvala što ste prošetali zimskim stazama!", new Location(45.2551, 19.8451));
        ArchivedWinterWalk.Publish();
        ArchivedWinterWalk.Archive();
    }

    public static SocialChallenge[] All =>
    [
        DraftEveningWalk, DraftMorningMeetup, PublishedNoonPicnic, RetiredSunsetGathering, ArchivedWinterWalk,
        PublishedAllDayStroll, PublishedAllDayGathering, PublishedAllDayMeetup, PublishedAllDayConcert
    ];
}
