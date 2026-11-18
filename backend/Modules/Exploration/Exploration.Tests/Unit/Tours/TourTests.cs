using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);
    private static readonly TourPrice FreePrice = new(true, null);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));

    private static void AddKeyPointAt(Tour tour, string name, double latitude, double longitude)
    {
        tour.AddKeyPoint(name, "Opis tačke.", "/images/exploration/tacka.jpg", new Location(latitude, longitude), null, false);
    }

    private static Tour CreatePublishableTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPointAt(tour, "Prva", 45.25, 19.86);
        AddKeyPointAt(tour, "Druga", 45.26, 19.87);
        return tour;
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreatePublishableTour();
        tour.Publish(FreePrice);
        return tour;
    }

    private static Tour CreateArchivedTour()
    {
        var tour = CreatePublishedTour();
        tour.Archive();
        return tour;
    }

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
        tour.KeyPoints.Should().BeEmpty();
        tour.Length.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, [], new RecommendedGroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_is_published_when_all_rules_are_met()
    {
        var tour = CreatePublishableTour();

        tour.Publish(FreePrice);

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Published_tour_keeps_the_price_it_was_published_with()
    {
        var tour = CreatePublishableTour();

        tour.Publish(new TourPrice(false, 50m));

        tour.Price.Should().Be(new TourPrice(false, 50m));
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_with_a_single_key_point_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPointAt(tour, "Jedina", 45.25, 19.86);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreatePublishedTour();

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published()
    {
        var tour = CreateArchivedTour();

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_is_archived_with_an_archive_time_and_without_a_publish_time()
    {
        var tour = CreatePublishedTour();

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().NotBeNull();
        tour.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void Draft_tour_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_archived_again()
    {
        var tour = CreateArchivedTour();

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_is_reactivated_as_published_with_a_new_publish_time_and_without_an_archive_time()
    {
        var tour = CreateArchivedTour();
        var reactivationStart = DateTime.UtcNow;

        tour.Reactivate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().BeOnOrAfter(reactivationStart);
        tour.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public void Reactivated_tour_has_the_same_price_as_before_archiving()
    {
        var tour = CreatePublishableTour();
        tour.Publish(new TourPrice(false, 50m));
        tour.Archive();

        tour.Reactivate();

        tour.Price.Should().Be(new TourPrice(false, 50m));
    }

    [Fact]
    public void Draft_tour_cannot_be_reactivated()
    {
        var tour = CreateTour(LongDescription);

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_reactivated()
    {
        var tour = CreatePublishedTour();

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Opis tačke.", "/images/exploration/tacka.jpg")]
    [InlineData("   ", "Opis tačke.", "/images/exploration/tacka.jpg")]
    [InlineData("Tačka", "", "/images/exploration/tacka.jpg")]
    [InlineData("Tačka", "   ", "/images/exploration/tacka.jpg")]
    [InlineData("Tačka", "Opis tačke.", "")]
    [InlineData("Tačka", "Opis tačke.", "   ")]
    public void Key_point_requires_a_name_a_description_and_an_image(string name, string description, string image)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddKeyPoint(name, description, image, new Location(45.25, 19.86), null, false);

        addition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Key_point_requires_a_location()
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddKeyPoint("Tačka", "Opis tačke.", "/images/exploration/tacka.jpg", null!, null, false);

        addition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void New_key_point_takes_the_next_place_in_the_list()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 45.25, 19.86);
        AddKeyPointAt(tour, "Druga", 45.26, 19.87);

        tour.AddKeyPoint("Treća", "Opis tačke.", "/images/exploration/treca.jpg", new Location(45.27, 19.88), "Tajna", true);

        var added = tour.KeyPoints[^1];
        added.Name.Should().Be("Treća");
        added.Order.Should().Be(3);
        added.Secret.Should().Be("Tajna");
        added.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void Removing_a_middle_key_point_leaves_the_order_without_gaps()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 45.25, 19.86);
        AddKeyPointAt(tour, "Druga", 45.26, 19.87);
        AddKeyPointAt(tour, "Treća", 45.27, 19.88);
        var middle = tour.KeyPoints[1];

        tour.RemoveKeyPoint(middle.Id);

        tour.KeyPoints.Select(keyPoint => keyPoint.Name).Should().Equal("Prva", "Treća");
        tour.KeyPoints.Select(keyPoint => keyPoint.Order).Should().Equal(1, 2);
    }

    // One degree of longitude on the equator is about 111.19 km.
    [Fact]
    public void Length_of_a_tour_with_three_key_points_is_the_rounded_sum_of_the_distances_between_consecutive_points()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 0, 0);
        AddKeyPointAt(tour, "Druga", 0, 1);

        tour.AddKeyPoint("Treća", "Opis tačke.", "/images/exploration/tacka.jpg", new Location(0, 2), null, false);

        tour.Length.Should().Be(new TourLength(222.4m));
    }

    [Fact]
    public void Moving_a_key_point_changes_the_tour_length()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 0, 0);
        AddKeyPointAt(tour, "Druga", 0, 1);
        var second = tour.KeyPoints[1];

        tour.ChangeKeyPoint(second.Id, "Druga", "Opis tačke.", null, new Location(0, 2), null, false);

        tour.Length.Should().Be(new TourLength(222.4m));
    }

    [Fact]
    public void Tour_left_with_a_single_key_point_has_no_length()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 0, 0);
        AddKeyPointAt(tour, "Druga", 0, 1);
        var second = tour.KeyPoints[1];

        tour.RemoveKeyPoint(second.Id);

        tour.Length.Should().BeNull();
    }

    [Fact]
    public void Two_key_points_at_the_same_location_make_a_tour_of_zero_length()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPointAt(tour, "Prva", 45.25, 19.86);

        tour.AddKeyPoint("Druga", "Opis tačke.", "/images/exploration/tacka.jpg", new Location(45.25, 19.86), null, false);

        tour.Length.Should().Be(new TourLength(0m));
    }

    [Fact]
    public void Key_point_cannot_be_added_to_a_published_tour()
    {
        var tour = CreatePublishedTour();

        var addition = () => tour.AddKeyPoint("Treća", "Opis tačke.", "/images/exploration/tacka.jpg", new Location(45.27, 19.88), null, false);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_point_of_a_published_tour_cannot_be_changed()
    {
        var tour = CreatePublishedTour();
        var first = tour.KeyPoints[0];

        var change = () => tour.ChangeKeyPoint(first.Id, "Nov naziv", "Opis tačke.", null, new Location(45.27, 19.88), null, false);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_point_of_a_published_tour_cannot_be_removed()
    {
        var tour = CreatePublishedTour();
        var first = tour.KeyPoints[0];

        var removal = () => tour.RemoveKeyPoint(first.Id);

        removal.Should().Throw<DomainException>();
    }
}
