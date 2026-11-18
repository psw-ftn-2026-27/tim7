using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Contracts;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<AuthorTourDto?> GetAuthorTourAsync(Guid tourId, Guid authorId);
    Task<List<string>> GetPublishedTagsAsync();
    Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize);
    Task<PublishedTourDto?> GetPublishedTourAsync(Guid tourId);
    Task<TourForSaleDto?> GetTourForSaleAsync(Guid tourId);
}
