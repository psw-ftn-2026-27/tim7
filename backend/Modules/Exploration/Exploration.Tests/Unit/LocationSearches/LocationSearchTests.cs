using Exploration.Domain.LocationSearches;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.LocationSearches;

public class LocationSearchTests
{
    [Fact]
    public void Location_search_with_a_negative_number_of_results_is_rejected()
    {
        var creation = () => new LocationSearch(new Location(45.2551, 19.8451), new SearchRadius(5), null, null, null, null, null, null, null, null, -1);

        creation.Should().Throw<DomainException>();
    }
}
