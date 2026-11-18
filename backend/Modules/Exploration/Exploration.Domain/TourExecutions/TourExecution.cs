using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// A tourist's session of walking a tour. It refers to the tourist and the tour by ID only.
public sealed class TourExecution : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public Location StartLocation { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    private TourExecution()
    {
        StartLocation = null!;
    }

    public TourExecution(Guid touristId, Guid tourId, Location startLocation)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        TourId = tourId;
        StartLocation = startLocation;
        Status = TourExecutionStatus.Active;
        StartedAt = DateTime.UtcNow;
        LastActivityAt = StartedAt;
    }

    public void Abandon()
    {
        if (Status != TourExecutionStatus.Active)
        {
            throw new DomainException("Only an active tour can be abandoned.");
        }

        Status = TourExecutionStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }
}
