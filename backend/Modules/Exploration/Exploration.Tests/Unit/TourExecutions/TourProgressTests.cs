using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourProgressTests
{
    private const double EarthRadiusInMeters = 6371000;

    private static readonly Location FirstKeyPointLocation = new(45.2500, 19.8600);
    private static readonly Location SecondKeyPointLocation = new(45.2600, 19.8700);

    private readonly TourProgress _tourProgress = new();

    private static Tour CreateTour(Location firstKeyPointLocation, Location secondKeyPointLocation)
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));
        tour.AddKeyPoint("Prva", "Opis tačke.", "/images/exploration/tacka.jpg", firstKeyPointLocation, "Prva tajna.", true);
        tour.AddKeyPoint("Druga", "Opis tačke.", "/images/exploration/tacka.jpg", secondKeyPointLocation, "Druga tajna.", false);
        return tour;
    }

    private static TourExecution CreateActiveExecution(Tour tour)
    {
        return new TourExecution(WellKnownUsers.Explorer, tour.Id, new Location(45.2400, 19.8500));
    }

    // Moving due north changes only the latitude, so the air distance is exactly the arc along the meridian.
    private static Location NorthOf(Location location, double meters)
    {
        return new Location(location.Latitude + meters / EarthRadiusInMeters * 180 / Math.PI, location.Longitude);
    }

    [Fact]
    public void A_tourist_near_a_later_key_point_does_not_complete_it_while_the_first_by_order_is_not_completed()
    {
        var tour = CreateTour(FirstKeyPointLocation, SecondKeyPointLocation);
        var execution = CreateActiveExecution(tour);

        _tourProgress.CheckPosition(execution, tour, SecondKeyPointLocation);

        execution.KeyPointCompletions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(49, true)]
    [InlineData(51, false)]
    public void A_key_point_is_completed_only_when_the_tourist_is_at_most_fifty_meters_from_it(double meters, bool isCompleted)
    {
        var tour = CreateTour(FirstKeyPointLocation, SecondKeyPointLocation);
        var execution = CreateActiveExecution(tour);

        _tourProgress.CheckPosition(execution, tour, NorthOf(FirstKeyPointLocation, meters));

        execution.HasCompleted(tour.KeyPoints[0].Id).Should().Be(isCompleted);
    }

    [Fact]
    public void One_check_near_two_key_points_completes_only_the_first_of_them()
    {
        var tour = CreateTour(FirstKeyPointLocation, NorthOf(FirstKeyPointLocation, 20));
        var execution = CreateActiveExecution(tour);

        _tourProgress.CheckPosition(execution, tour, NorthOf(FirstKeyPointLocation, 10));

        execution.KeyPointCompletions.Should().ContainSingle()
            .Which.KeyPointId.Should().Be(tour.KeyPoints[0].Id);
    }

    [Fact]
    public void A_check_far_from_every_key_point_still_records_the_position_and_the_activity_time()
    {
        var tour = CreateTour(FirstKeyPointLocation, SecondKeyPointLocation);
        var execution = CreateActiveExecution(tour);
        var position = new Location(45.3000, 19.9500);

        _tourProgress.CheckPosition(execution, tour, position);

        execution.KeyPointCompletions.Should().BeEmpty();
        execution.LastKnownLocation.Should().Be(position);
        execution.LastActivityAt.Should().BeAfter(execution.StartedAt);
    }
}
