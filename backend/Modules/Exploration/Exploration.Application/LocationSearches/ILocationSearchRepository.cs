using Exploration.Domain.LocationSearches;

namespace Exploration.Application.LocationSearches;

public interface ILocationSearchRepository
{
    void Add(LocationSearch locationSearch);
}
