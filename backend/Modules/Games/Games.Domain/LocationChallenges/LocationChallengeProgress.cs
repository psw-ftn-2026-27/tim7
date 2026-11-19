using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallengeProgress : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid LocationChallengeId { get; private set; }
    private readonly List<Guid> _discoveredPointIds = [];
    public IReadOnlyList<Guid> DiscoveredPointIds => _discoveredPointIds;
    public DateTime? CompletedAt { get; private set; }
    public int EarnedXp { get; private set; }

    public bool IsCompleted => CompletedAt is not null;

    private LocationChallengeProgress() { }

    public LocationChallengeProgress(Guid userId, Guid locationChallengeId)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        LocationChallengeId = locationChallengeId;
    }

    public void EnsureCanActivate()
    {
        if (IsCompleted)
        {
            throw new DomainException("A completed challenge cannot be activated again.");
        }
    }

    // The challenge tells how many points it has and how much XP it brings.
    public void Discover(Guid pointId, int pointCount, int xp)
    {
        EnsureCanActivate();

        if (_discoveredPointIds.Contains(pointId))
        {
            return;
        }

        _discoveredPointIds.Add(pointId);

        if (_discoveredPointIds.Count == pointCount)
        {
            CompletedAt = DateTime.UtcNow;
            EarnedXp = xp;
        }
    }
}
