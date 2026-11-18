using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionStarterTests
{
    private static readonly Location StartLocation = new(45.25, 19.85);

    private readonly TourExecutionStarter _starter = new();

    private static Tour CreatePublishedTour(TourPrice price)
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Šetnja tvrđavom", new string('o', 100), TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.AddKeyPoint("Prva", "Opis tačke.", "/images/exploration/tacka.jpg", new Location(45.25, 19.86), null, true);
        tour.AddKeyPoint("Druga", "Opis tačke.", "/images/exploration/tacka.jpg", new Location(45.26, 19.87), null, false);
        tour.Publish(price);
        return tour;
    }

    [Fact]
    public void A_tour_that_is_neither_free_nor_purchased_cannot_be_started()
    {
        var tour = CreatePublishedTour(new TourPrice(false, 50m));

        var start = () => _starter.Start(WellKnownUsers.Explorer, tour, StartLocation, false, null);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_free_tour_can_be_started_without_a_purchase()
    {
        var tour = CreatePublishedTour(new TourPrice(true, null));

        var execution = _starter.Start(WellKnownUsers.Explorer, tour, StartLocation, false, null);

        execution.TourId.Should().Be(tour.Id);
        execution.Status.Should().Be(TourExecutionStatus.Active);
    }

    [Fact]
    public void Starting_the_tour_that_is_already_active_returns_the_existing_session_instead_of_a_new_one()
    {
        var tour = CreatePublishedTour(new TourPrice(false, 50m));
        var activeExecution = new TourExecution(WellKnownUsers.Explorer, tour.Id, StartLocation);

        var execution = _starter.Start(WellKnownUsers.Explorer, tour, new Location(45.30, 19.90), true, activeExecution);

        execution.Should().BeSameAs(activeExecution);
    }

    [Fact]
    public void Another_tour_cannot_be_started_while_one_is_active()
    {
        var activeTour = CreatePublishedTour(new TourPrice(false, 50m));
        var otherTour = CreatePublishedTour(new TourPrice(false, 50m));
        var activeExecution = new TourExecution(WellKnownUsers.Explorer, activeTour.Id, StartLocation);

        var start = () => _starter.Start(WellKnownUsers.Explorer, otherTour, StartLocation, true, activeExecution);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_tourist_without_an_active_session_gets_a_new_active_session_at_the_start_location()
    {
        var tour = CreatePublishedTour(new TourPrice(false, 50m));

        var execution = _starter.Start(WellKnownUsers.Explorer, tour, StartLocation, true, null);

        execution.TouristId.Should().Be(WellKnownUsers.Explorer);
        execution.TourId.Should().Be(tour.Id);
        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.StartLocation.Should().Be(StartLocation);
    }
}
