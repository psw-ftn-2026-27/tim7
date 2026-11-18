using FluentAssertions;
using Identity.Core.UserPositions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Identity.Tests.Unit.UserPositions;

public class CoordinatesTests
{
    [Theory]
    [InlineData(90.1, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(0, 180.1)]
    [InlineData(0, -180.1)]
    public void Coordinates_with_a_latitude_or_longitude_out_of_range_cannot_be_created(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    [InlineData(90, -180)]
    [InlineData(-90, 180)]
    public void Coordinates_on_the_boundary_of_the_range_can_be_created(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().NotThrow();
    }
}
