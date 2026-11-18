using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftEveningWalk = new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, "Hvala što ste se okupili na keju!");
    public static readonly SocialChallenge DraftMorningMeetup = new("Jutarnje druženje", new TimeOnly(7, 0), new TimeOnly(9, 30), 4, 5, 50, "Dobro jutro, vidimo se sutra opet!");

    public static SocialChallenge[] All => [DraftEveningWalk, DraftMorningMeetup];
}
