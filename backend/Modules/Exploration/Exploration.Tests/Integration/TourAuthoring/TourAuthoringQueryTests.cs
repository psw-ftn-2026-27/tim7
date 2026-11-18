using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringQueryTests : BaseIntegrationTest
{
    public TourAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().HaveCount(TourSeed.All.Count(tour => tour.AuthorId == WellKnownUsers.Explorer));
        tours.Should().OnlyContain(tour => tour.AuthorId == WellKnownUsers.Explorer);
        tours.Should().NotContain(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
        var seededGroupSize = TourSeed.FortressWalk.RecommendedGroupSize;
        var fortressWalk = tours.Should().ContainSingle(tour => tour.Id == TourSeed.FortressWalk.Id).Which;
        fortressWalk.RecommendedGroupSize.Should().Be(new RecommendedGroupSizeDto(seededGroupSize.From, seededGroupSize.To));
        tours.Should().ContainSingle(tour => tour.Id == TourSeed.PublishableRiverside.Id)
            .Which.Length.Should().Be(new TourLengthDto(TourSeed.PublishableRiverside.Length!.Kilometers));
        tours.Should().ContainSingle(tour => tour.Id == TourSeed.CityStroll.Id)
            .Which.Length.Should().BeNull();
        var archived = tours.Should().ContainSingle(tour => tour.Id == TourSeed.ArchivedLakeside.Id).Which;
        archived.ArchivedAt.Should().NotBeNull();
        archived.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task Author_sees_the_price_of_a_published_tour_and_no_price_for_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        var seededPrice = TourSeed.PublishedVineyards.Price!;
        var published = tours.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id).Which;
        published.IsFree.Should().Be(seededPrice.IsFree);
        published.Amount.Should().Be(seededPrice.Amount);
        var draft = tours.Should().ContainSingle(tour => tour.Id == TourSeed.FortressWalk.Id).Which;
        draft.IsFree.Should().BeNull();
        draft.Amount.Should().BeNull();
    }
}
