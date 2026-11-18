using Exploration.Application.LocationSearches;
using Exploration.Domain.LocationSearches;

namespace Exploration.Infrastructure.Persistence.LocationSearches;

internal sealed class LocationSearchRepository : ILocationSearchRepository
{
    private readonly ExplorationDbContext _dbContext;

    public LocationSearchRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(LocationSearch locationSearch)
    {
        _dbContext.LocationSearches.Add(locationSearch);
    }
}
