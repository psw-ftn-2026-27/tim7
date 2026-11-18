using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.LocationSearches;

// A location search a tourist ran, kept for analytics. It is never changed after it is created,
// and it deliberately has no user field, so the record cannot tell who searched.
public sealed class LocationSearch : AggregateRoot
{
    public Location Center { get; private set; }
    public SearchRadius Radius { get; private set; }
    public string? Name { get; private set; }
    public List<TourDifficulty>? Difficulties { get; private set; }
    public List<string>? Tags { get; private set; }
    public TransportMode? Transport { get; private set; }
    public int? MinPrice { get; private set; }
    public int? MaxPrice { get; private set; }
    public decimal? MinLength { get; private set; }
    public decimal? MaxLength { get; private set; }
    public int ResultCount { get; private set; }
    public DateTime SearchedAt { get; private set; }

    private LocationSearch()
    {
        Center = null!;
        Radius = null!;
    }

    public LocationSearch(Location center, SearchRadius radius, string? name, List<TourDifficulty>? difficulties,
        List<string>? tags, TransportMode? transport, int? minPrice, int? maxPrice, decimal? minLength,
        decimal? maxLength, int resultCount)
        : base(Guid.NewGuid())
    {
        if (center is null)
        {
            throw new DomainException("A location search requires a center.");
        }
        if (radius is null)
        {
            throw new DomainException("A location search requires a radius.");
        }
        if (resultCount < 0)
        {
            throw new DomainException("A location search cannot have a negative number of results.");
        }

        Center = center;
        Radius = radius;
        Name = name;
        Difficulties = difficulties;
        Tags = tags;
        Transport = transport;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        MinLength = minLength;
        MaxLength = maxLength;
        ResultCount = resultCount;
        SearchedAt = DateTime.UtcNow;
    }
}
