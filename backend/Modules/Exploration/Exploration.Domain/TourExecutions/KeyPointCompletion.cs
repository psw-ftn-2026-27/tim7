namespace Exploration.Domain.TourExecutions;

// A key point the tourist reached in this session. It refers to the tour's key point by ID only.
public sealed record KeyPointCompletion
{
    public Guid KeyPointId { get; private init; }
    public DateTime CompletedAt { get; private init; }

    private KeyPointCompletion() { }

    public KeyPointCompletion(Guid keyPointId, DateTime completedAt)
    {
        KeyPointId = keyPointId;
        CompletedAt = completedAt;
    }
}
