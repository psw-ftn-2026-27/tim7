using Exploration.Application.TourExecutions;
using Exploration.Application.TourExecutionTracking;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionReadRepository : ITourExecutionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The tour is read whatever its status, and its first key point is the first by order whether it is public or not.
    public Task<ActiveTourExecutionDto?> GetActiveByTouristAsync(Guid touristId)
    {
        return _dbContext.TourExecutions
            .Where(execution => execution.TouristId == touristId && execution.Status == TourExecutionStatus.Active)
            .Join(_dbContext.Tours,
                execution => execution.TourId,
                tour => tour.Id,
                (execution, tour) => new ActiveTourExecutionDto(
                    execution.Id,
                    tour.Id,
                    tour.Name,
                    new LocationDto(execution.StartLocation.Latitude, execution.StartLocation.Longitude),
                    tour.KeyPoints
                        .OrderBy(keyPoint => keyPoint.Order)
                        .Select(keyPoint => new TourExecutionKeyPointDto(
                            keyPoint.Id,
                            keyPoint.Name,
                            keyPoint.Description,
                            keyPoint.Image,
                            new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                        .FirstOrDefault()))
            .FirstOrDefaultAsync();
    }
}
