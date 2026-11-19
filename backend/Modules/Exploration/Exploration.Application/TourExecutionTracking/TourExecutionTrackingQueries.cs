using Exploration.Application.TourExecutions;

namespace Exploration.Application.TourExecutionTracking;

public sealed class TourExecutionTrackingQueries
{
    private readonly ITourExecutionReadRepository _tourExecutionReadRepository;

    public TourExecutionTrackingQueries(ITourExecutionReadRepository tourExecutionReadRepository)
    {
        _tourExecutionReadRepository = tourExecutionReadRepository;
    }

    public async Task<MyActiveTourExecutionDto> GetActiveAsync(Guid touristId)
    {
        var execution = await _tourExecutionReadRepository.GetActiveByTouristAsync(touristId);

        return new MyActiveTourExecutionDto(execution);
    }

    public Task<List<DiscoveredSecretDto>> GetDiscoveredSecretsAsync(Guid tourId, Guid touristId)
    {
        return _tourExecutionReadRepository.GetDiscoveredSecretsAsync(tourId, touristId);
    }
}
