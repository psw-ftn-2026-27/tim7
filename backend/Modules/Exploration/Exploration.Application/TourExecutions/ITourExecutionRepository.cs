using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionRepository
{
    Task<TourExecution?> GetActiveByTouristAsync(Guid touristId);
    void Add(TourExecution tourExecution);
}
