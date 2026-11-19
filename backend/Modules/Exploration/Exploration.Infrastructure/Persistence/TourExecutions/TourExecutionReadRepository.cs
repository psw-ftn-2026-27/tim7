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

    // The tour is read whatever its status, and its next key point is the first uncompleted by order whether it is public or not.
    // Only the completed key points carry their secrets.
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
                    new LocationDto(execution.LastKnownLocation.Latitude, execution.LastKnownLocation.Longitude),
                    tour.KeyPoints
                        .Where(keyPoint => execution.KeyPointCompletions.Any(completion => completion.KeyPointId == keyPoint.Id))
                        .OrderBy(keyPoint => keyPoint.Order)
                        .Select(keyPoint => new CompletedKeyPointDto(
                            keyPoint.Id,
                            keyPoint.Order,
                            keyPoint.Name,
                            execution.KeyPointCompletions
                                .Where(completion => completion.KeyPointId == keyPoint.Id)
                                .Select(completion => completion.CompletedAt)
                                .First(),
                            keyPoint.Secret))
                        .ToList(),
                    tour.KeyPoints
                        .Where(keyPoint => !execution.KeyPointCompletions.Any(completion => completion.KeyPointId == keyPoint.Id))
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

    // Completions from all of the tourist's sessions of the tour, whatever their status; each key point once, with its earliest completion.
    public Task<List<DiscoveredSecretDto>> GetDiscoveredSecretsAsync(Guid tourId, Guid touristId)
    {
        var completions = _dbContext.TourExecutions
            .Where(execution => execution.TourId == tourId && execution.TouristId == touristId)
            .SelectMany(execution => execution.KeyPointCompletions);

        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .SelectMany(tour => tour.KeyPoints)
            .Where(keyPoint => completions.Any(completion => completion.KeyPointId == keyPoint.Id))
            .OrderBy(keyPoint => keyPoint.Order)
            .Select(keyPoint => new DiscoveredSecretDto(
                keyPoint.Id,
                keyPoint.Name,
                keyPoint.Secret,
                completions
                    .Where(completion => completion.KeyPointId == keyPoint.Id)
                    .Min(completion => completion.CompletedAt)))
            .ToListAsync();
    }
}
