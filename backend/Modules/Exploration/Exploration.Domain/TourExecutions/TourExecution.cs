using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// A tourist's session of walking a tour. It refers to the tourist and the tour by ID only.
public sealed class TourExecution : AggregateRoot
{
    private readonly List<KeyPointCompletion> _keyPointCompletions = [];

    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public Location StartLocation { get; private set; }
    public Location LastKnownLocation { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public IReadOnlyList<KeyPointCompletion> KeyPointCompletions => _keyPointCompletions;

    private TourExecution()
    {
        StartLocation = null!;
        LastKnownLocation = null!;
    }

    public TourExecution(Guid touristId, Guid tourId, Location startLocation)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        TourId = tourId;
        StartLocation = startLocation;
        // A copy, not the same instance: EF stores each owned Location as its own value and rejects one instance in two places.
        LastKnownLocation = new Location(startLocation.Latitude, startLocation.Longitude);
        Status = TourExecutionStatus.Active;
        StartedAt = DateTime.UtcNow;
        LastActivityAt = StartedAt;
    }

    public bool HasCompleted(Guid keyPointId)
    {
        return _keyPointCompletions.Any(completion => completion.KeyPointId == keyPointId);
    }

    public void RecordPosition(Location position)
    {
        EnsureIsActive("Only an active tour can record a position.");

        LastKnownLocation = position;
        LastActivityAt = DateTime.UtcNow;
    }

    // Completing the last of the tour's key points completes the session.
    public void CompleteKeyPoint(Guid keyPointId, int keyPointCount)
    {
        EnsureIsActive("Only an active tour can complete a key point.");
        if (HasCompleted(keyPointId))
        {
            throw new DomainException("The key point is already completed.");
        }

        _keyPointCompletions.Add(new KeyPointCompletion(keyPointId, DateTime.UtcNow));
        if (_keyPointCompletions.Count >= keyPointCount)
        {
            Status = TourExecutionStatus.Completed;
            EndedAt = DateTime.UtcNow;
        }
    }

    public void Abandon()
    {
        EnsureIsActive("Only an active tour can be abandoned.");

        Status = TourExecutionStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }

    private void EnsureIsActive(string message)
    {
        if (Status != TourExecutionStatus.Active)
        {
            throw new DomainException(message);
        }
    }
}
