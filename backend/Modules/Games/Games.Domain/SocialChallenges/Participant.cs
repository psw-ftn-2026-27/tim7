using Shared.Domain;

namespace Games.Domain.SocialChallenges;

// An explorer who joined a run of a social challenge, with what their last position check showed.
public sealed class Participant : Entity
{
    public Guid UserId { get; private set; }
    public bool IsInRange { get; private set; }
    // Empty until the participant sends their first position.
    public DateTime? LastCheckedAt { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? EarnedXp { get; private set; }

    private Participant() { }

    internal Participant(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    internal void Check(bool isInRange, DateTime checkedAt)
    {
        IsInRange = isInRange;
        LastCheckedAt = checkedAt;
    }

    internal bool IsActiveAt(DateTime now)
    {
        return IsInRange
            && LastCheckedAt is not null
            && now - LastCheckedAt.Value <= SocialChallengeRun.MaxCheckAge;
    }

    internal void Solve(DateTime solvedAt, int xp)
    {
        SolvedAt = solvedAt;
        EarnedXp = xp;
    }
}
