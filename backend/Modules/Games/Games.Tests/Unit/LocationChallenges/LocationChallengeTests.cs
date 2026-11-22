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

    [Fact]
    public void Radius_is_the_distance_to_the_farthest_hidden_point_plus_the_bonus()
    {
        var challenge = new LocationChallenge("Potraga", 100, Start, ValidPoints(), 50);

        var radius = challenge.RadiusMeters();

        radius.Should().BeApproximately(272.4, 1);
        challenge.RadiusMeters().Should().Be(radius);
    }

    [Theory]
    [InlineData(0.0024, true)]
    [InlineData(0.0030, false)]
    public void Position_is_in_the_circle_only_within_the_radius(double degreesNorthOfStart, bool expected)
    {
        var challenge = new LocationChallenge("Potraga", 100, Start, ValidPoints(), 50);

        var isInCircle = challenge.IsInCircle(NorthOfStart(degreesNorthOfStart));

        isInCircle.Should().Be(expected);
    }

    [Fact]
    public void Undiscovered_point_is_found_within_5_meters()
    {
        var challenge = new LocationChallenge("Potraga", 100, Start, ValidPoints(), 50);

        var point = challenge.GetUndiscoveredPointNear(NorthOfStart(0.001 + 0.00003), []);

        point.Should().Be(challenge.HiddenPoints[0]);
    }

    [Fact]
    public void Undiscovered_point_is_not_found_farther_than_5_meters()
    {
        var challenge = new LocationChallenge("Potraga", 100, Start, ValidPoints(), 50);

        var find = () => challenge.GetUndiscoveredPointNear(NorthOfStart(0.001 + 0.00006), []);

        find.Should().Throw<DomainException>();
    }

    [Fact]
    public void Discovered_point_is_not_found_again()
    {
        var challenge = new LocationChallenge("Potraga", 100, Start, ValidPoints(), 50);

        var find = () => challenge.GetUndiscoveredPointNear(NorthOfStart(0.001), [challenge.HiddenPoints[0].Id]);

        find.Should().Throw<DomainException>();
    }
}
