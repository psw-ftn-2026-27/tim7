using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeTests
{
    // One thousandth of a degree of latitude is about 111 m.
    private static readonly Location Start = new(44.8000, 20.4600);

    private static Location NorthOfStart(double degrees) => new(44.8000 + degrees, 20.4600);

    private static HiddenPoint PointNorthOfStart(double degrees) => new("/images/games/point.png", NorthOfStart(degrees));

    private static List<HiddenPoint> ValidPoints() => [PointNorthOfStart(0.001), PointNorthOfStart(0.002)];

    [Theory]
    [InlineData("ab")]
    [InlineData("Naziv koji ima više od dvadeset karaktera")]
    public void Challenge_cannot_be_created_with_a_name_outside_three_to_twenty_characters(string name)
    {
        var create = () => new LocationChallenge(name, 100, Start, ValidPoints(), 50);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Challenge_cannot_be_created_with_a_non_positive_xp(int xp)
    {
        var create = () => new LocationChallenge("Potraga", xp, Start, ValidPoints(), 50);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_cannot_be_created_without_hidden_points()
    {
        var create = () => new LocationChallenge("Potraga", 100, Start, [], 50);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Hidden_point_cannot_be_created_without_a_photo()
    {
        var create = () => new HiddenPoint("", NorthOfStart(0.001));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_cannot_be_created_with_a_point_farther_than_400_meters_from_the_start()
    {
        List<HiddenPoint> points =
        [
            PointNorthOfStart(0.001),
            PointNorthOfStart(0.002),
            PointNorthOfStart(0.003),
            PointNorthOfStart(0.0045)
        ];

        var create = () => new LocationChallenge("Potraga", 100, Start, points, 50);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_cannot_be_created_with_two_locations_closer_than_10_meters()
    {
        List<HiddenPoint> points = [PointNorthOfStart(0.001), PointNorthOfStart(0.00104)];

        var create = () => new LocationChallenge("Potraga", 100, Start, points, 50);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_cannot_be_created_with_a_location_that_has_no_other_location_within_200_meters()
    {
        List<HiddenPoint> points = [PointNorthOfStart(0.003)];

        var create = () => new LocationChallenge("Potraga", 100, Start, points, 50);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Challenge_cannot_be_created_with_a_radius_bonus_outside_zero_to_100_meters(int radiusBonusMeters)
    {
        var create = () => new LocationChallenge("Potraga", 100, Start, ValidPoints(), radiusBonusMeters);

        create.Should().Throw<DomainException>();
    }
}
