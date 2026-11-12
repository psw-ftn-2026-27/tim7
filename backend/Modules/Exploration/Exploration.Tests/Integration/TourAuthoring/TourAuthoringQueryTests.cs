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
        fortressWalk.Length.Should().Be(new TourLengthDto(TourSeed.FortressWalk.Length!.Kilometers));
        tours.Should().ContainSingle(tour => tour.Id == TourSeed.CityStroll.Id)
            .Which.Length.Should().BeNull();
        var archived = tours.Should().ContainSingle(tour => tour.Id == TourSeed.ArchivedLakeside.Id).Which;
        archived.ArchivedAt.Should().NotBeNull();
        archived.PublishedAt.Should().BeNull();
    }
}
