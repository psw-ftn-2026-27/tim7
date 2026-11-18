using Exploration.Domain.LocationSearches;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.LocationSearches;

public class SearchRadiusTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-2.5)]
    public void Radius_is_rejected_when_it_is_zero_or_negative(double kilometers)
    {
        var creation = () => new SearchRadius(kilometers);

        creation.Should().Throw<DomainException>();
    }
}
