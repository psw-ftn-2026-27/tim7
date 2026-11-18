using Exploration.Application.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(search, page, pageSize);
    }

    // A tour that is not published and a missing tour give the same answer, so the response does not reveal that the tour exists.
    public async Task<PublishedTourDto> GetPublishedTourAsync(Guid tourId)
    {
        return await _tourReadRepository.GetPublishedTourAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
    }
}
