using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
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

        return tours;
    }

    private static IQueryable<Tour> OrderBySearch(IQueryable<Tour> tours, TourSearchDto search)
    {
        // Difficulty is stored as text, so ordering by the column would be alphabetical
        // (Easy, Hard, Moderate). The rank spells out the intended order instead.
        if (search.DifficultySort == SortDirection.Ascending)
        {
            return tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt);
        }
        if (search.DifficultySort == SortDirection.Descending)
        {
            return tours
                .OrderByDescending(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
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
                    .ToList()));
    }
}
