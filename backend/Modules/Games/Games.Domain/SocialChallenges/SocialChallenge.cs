using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    public string Name { get; private set; }
    public ActiveWindow ActiveWindow { get; private set; }
    public Gathering Gathering { get; private set; }
    public int Xp { get; private set; }
    public string ExitMessage { get; private set; }
    public SocialChallengeStatus Status { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        ActiveWindow = null!;
        Gathering = null!;
        ExitMessage = null!;
    }

    public SocialChallenge(
        string name,
        TimeOnly start,
        TimeOnly end,
        int people,
        int rangeMeters,
        int xp,
        string exitMessage)
        : base(Guid.NewGuid())
    {
        EnsureValidDetails(name, xp, exitMessage);

        Name = name;
        ActiveWindow = new ActiveWindow(start, end);
        Gathering = new Gathering(people, rangeMeters);
        Xp = xp;
        ExitMessage = exitMessage;
        Status = SocialChallengeStatus.Draft;
    }

    public void Update(
        string name,
        TimeOnly start,
        TimeOnly end,
        int people,
        int rangeMeters,
        int xp,
        string exitMessage)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft challenge can be updated.");
        }
        EnsureValidDetails(name, xp, exitMessage);
        var activeWindow = new ActiveWindow(start, end);
        var gathering = new Gathering(people, rangeMeters);

        Name = name;
        ActiveWindow = activeWindow;
        Gathering = gathering;
        Xp = xp;
        ExitMessage = exitMessage;
    }

    public void EnsureDeletable()
    {
        if (Status != SocialChallengeStatus.Draft && Status != SocialChallengeStatus.Archived)
        {
            throw new DomainException("Only a draft or archived challenge can be deleted.");
        }
    }

    private static void EnsureValidDetails(string name, int xp, string exitMessage)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            throw new DomainException("A challenge requires a name of at most 50 characters.");
        }
        if (xp <= 0)
        {
            throw new DomainException("The XP of a challenge must be positive.");
        }
        if (exitMessage is null || exitMessage.Length is < 20 or > 200)
        {
            throw new DomainException("The exit message must have between 20 and 200 characters.");
        }
    }
}
