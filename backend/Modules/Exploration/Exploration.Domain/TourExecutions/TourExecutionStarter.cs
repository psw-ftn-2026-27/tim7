using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// Starting a tour looks at the tourist's other sessions, so the rules live here and not in one TourExecution.
public sealed class TourExecutionStarter
{
    // Returns the active session when it is already this tour, and a new session otherwise.
    public TourExecution Start(Guid touristId, Tour tour, Location startLocation, bool hasPurchased, TourExecution? activeExecution)
    {
        if (tour.Price?.IsFree != true && !hasPurchased)
        {
            throw new DomainException("A tour can be started only when it is free or purchased.");
        }
        if (activeExecution is not null && activeExecution.TourId == tour.Id)
        {
            return activeExecution;
        }
        if (activeExecution is not null)
        {
            throw new DomainException("Another tour is already active. Continue or abandon it before starting a new one.");
        }

        return new TourExecution(touristId, tour.Id, startLocation);
    }
}
