using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

// One gathering of explorers at a social challenge. A challenge has at most one active run at a time.
public sealed class SocialChallengeRun : AggregateRoot
{
    // A participant counts as active only if their last check is at most this old.
    public static readonly TimeSpan MaxCheckAge = TimeSpan.FromSeconds(30);

    private readonly List<Participant> _participants = [];

    public Guid ChallengeId { get; private set; }
    public IReadOnlyList<Participant> Participants => _participants;
    public DateTime? CompletedAt { get; private set; }

    private SocialChallengeRun() { }

    public SocialChallengeRun(Guid challengeId, Guid userId) : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        _participants.Add(new Participant(userId));
    }

    public void Join(Guid userId, bool hasSolvedBefore)
    {
        EnsureActive();
        if (hasSolvedBefore)
        {
            throw new DomainException("A solved challenge cannot be activated again.");
        }
        if (_participants.Any(participant => participant.UserId == userId))
        {
            return;
        }

        _participants.Add(new Participant(userId));
    }

    // The challenge tells whether the position is in range, how many people it needs and how much XP it brings.
    public void ReportPosition(Guid userId, bool inRange, DateTime now, int requiredPeople, int xp)
    {
        EnsureActive();
        var participant = _participants.FirstOrDefault(item => item.UserId == userId)
            ?? throw new DomainException("Only a participant of the run can report a position.");

        participant.Check(inRange, now);

        var activeParticipants = _participants.Where(item => item.IsActiveAt(now)).ToList();
        if (activeParticipants.Count < requiredPeople)
        {
            return;
        }

        CompletedAt = now;
        foreach (var active in activeParticipants)
        {
            active.Solve(now, xp);
        }
    }

    private void EnsureActive()
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("The run is already completed.");
        }
    }
}
