using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Contracts;
using Exploration.Domain.LocationSearches;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public Task<AuthorTourDto?> GetAuthorTourAsync(Guid tourId, Guid authorId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new AuthorTourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                new RecommendedGroupSizeDto(tour.RecommendedGroupSize.From, tour.RecommendedGroupSize.To),
                tour.Length == null ? null : new TourLengthDto(tour.Length.Kilometers),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Order,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.Image,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude),
                        keyPoint.Secret,
                        keyPoint.IsPublic))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        var matching = FilterBySearch(
            _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published),
            search);

        var totalCount = await matching.CountAsync();
        var items = await ProjectToDtos(OrderBySearch(matching, search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public async Task<PageResult<TourOnMapDto>> GetPublishedOnMapAsync(TourSearchDto search, Location? center, SearchRadius? radius, int page, int pageSize)
    {
        var matching = FilterBySearch(
            _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published),
            search);
        if (center is not null && radius is not null)
        {
            var toursInCircle = await FindToursWithPublicKeyPointInCircleAsync(matching, center, radius);
            matching = matching.Where(tour => toursInCircle.Contains(tour.Id));
        }

        var totalCount = await matching.CountAsync();
        var tours = await ProjectToDtos(OrderBySearch(matching, search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pageTourIds = tours.Select(tour => tour.Id).ToList();
        var firstPublicLocations = await _dbContext.Tours
            .Where(tour => pageTourIds.Contains(tour.Id))
            .Select(tour => new
            {
                tour.Id,
                Location = tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsPublic)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude))
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(tour => tour.Id, tour => tour.Location);

        var items = tours
            .Select(tour => new TourOnMapDto(tour, firstPublicLocations[tour.Id]))
            .ToList();
        return new PageResult<TourOnMapDto>(items, totalCount);
    }

    public Task<PublishedTourDto?> GetPublishedTourAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.Status == TourStatus.Published)
            .Select(tour => new PublishedTourDto(
                tour.Id,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Length == null ? null : new TourLengthDto(tour.Length.Kilometers),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsPublic)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new PublicKeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.Image,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public Task<TourForSaleDto?> GetTourForSaleAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .Select(tour => new TourForSaleDto(
                tour.Id,
                tour.Name,
                tour.AuthorId,
                tour.Price == null ? null : tour.Price.IsFree,
                tour.Price == null ? null : tour.Price.Amount,
                tour.Status == TourStatus.Published))
            .FirstOrDefaultAsync();
    }

    public Task<List<string>> GetPublishedTagsAsync()
    {
        return _dbContext.Tours
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.Tags)
            .Distinct()
            .OrderBy(tag => tag)
            .ToListAsync();
    }
    private static IQueryable<Tour> FilterBySearch(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (search.Difficulties is not null && search.Difficulties.Count > 0)
        {
            var difficulties = search.Difficulties;
            tours = tours.Where(tour => difficulties.Contains(tour.Difficulty));
        }
        if (search.Tags is not null && search.Tags.Count > 0)
        {
            var tags = search.Tags.Select(tag => tag.ToLower()).ToList();
            tours = tours.Where(tour => tour.Tags.Any(tag => tags.Contains(tag.ToLower())));
        }
        if (search.Transport is not null)
        {
            var transport = search.Transport.Value;
            tours = tours.Where(tour => tour.TransportTimes.Any(time => time.Transport == transport));
        }
        // A free tour has no amount and counts as 0. A tour without a price is left out
        // as soon as either bound is set.
        if (search.MinPrice is not null)
        {
            var minPrice = search.MinPrice.Value;
            tours = tours.Where(tour => tour.Price != null && (tour.Price.Amount ?? 0) >= minPrice);
        }
        if (search.MaxPrice is not null)
        {
            var maxPrice = search.MaxPrice.Value;
            tours = tours.Where(tour => tour.Price != null && (tour.Price.Amount ?? 0) <= maxPrice);
        }

        // A tour without a length is left out as soon as either bound is set.
        if (search.MinLength is not null)
        {
            var minLength = search.MinLength.Value;
            tours = tours.Where(tour => tour.Length != null && tour.Length.Kilometers >= minLength);
        }
        if (search.MaxLength is not null)
        {
            var maxLength = search.MaxLength.Value;
            tours = tours.Where(tour => tour.Length != null && tour.Length.Kilometers <= maxLength);
        }

        return tours;
    }

    // The distance is measured with Location.DistanceTo, which the database cannot run, so the public key
    // points of the tours that pass the other criteria are read and checked here. Non-public key points are
    // never read, so a tour counts only through a public key point, and a tour without one never matches.
    private static async Task<List<Guid>> FindToursWithPublicKeyPointInCircleAsync(IQueryable<Tour> tours, Location center, SearchRadius radius)
    {
        var publicKeyPoints = await tours
            .SelectMany(tour => tour.KeyPoints
                .Where(keyPoint => keyPoint.IsPublic)
                .Select(keyPoint => new
                {
                    TourId = tour.Id,
                    keyPoint.Location.Latitude,
                    keyPoint.Location.Longitude
                }))
            .ToListAsync();

        return publicKeyPoints
            .Where(keyPoint => new Location(keyPoint.Latitude, keyPoint.Longitude).DistanceTo(center) <= radius.Kilometers)
            .Select(keyPoint => keyPoint.TourId)
            .Distinct()
            .ToList();
    }

    private static IQueryable<Tour> OrderBySearch(IQueryable<Tour> tours, TourSearchDto search)
    {
        // Difficulty is stored as text, so ordering by the column would be alphabetical
        // (Easy, Hard, Moderate). The rank spells out the intended order instead.
        if (search.SortBy == TourSort.DifficultyAscending)
        {
            return tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt);
        }
        if (search.SortBy == TourSort.DifficultyDescending)
        {
            return tours
                .OrderByDescending(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt);
        }
        // A tour without a price goes last in both directions; a free tour counts as 0.
        if (search.SortBy == TourSort.PriceAscending)
        {
            return tours
                .OrderBy(tour => tour.Price == null)
                .ThenBy(tour => tour.Price!.Amount ?? 0)
                .ThenByDescending(tour => tour.PublishedAt);
        }
        if (search.SortBy == TourSort.PriceDescending)
        {
            return tours
                .OrderBy(tour => tour.Price == null)
                .ThenByDescending(tour => tour.Price!.Amount ?? 0)
                .ThenByDescending(tour => tour.PublishedAt);
        }

        return tours.OrderByDescending(tour => tour.PublishedAt);
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                new RecommendedGroupSizeDto(tour.RecommendedGroupSize.From, tour.RecommendedGroupSize.To),
                tour.Length == null ? null : new TourLengthDto(tour.Length.Kilometers),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.Price == null ? null : tour.Price.IsFree,
                tour.Price == null ? null : tour.Price.Amount));
    }
}
