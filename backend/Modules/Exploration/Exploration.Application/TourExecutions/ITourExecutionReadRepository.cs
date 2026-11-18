using Exploration.Application.TourExecutionTracking;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    Task<ActiveTourExecutionDto?> GetActiveByTouristAsync(Guid touristId);
}
