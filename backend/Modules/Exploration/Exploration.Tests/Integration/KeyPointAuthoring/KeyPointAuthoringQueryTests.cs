using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringQueryTests : BaseIntegrationTest
{
    public KeyPointAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_gets_their_tour_with_its_key_points_in_order_including_the_secrets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TourSeed.PublishableRiverside;

        var tour = await client.GetFromJsonAsync<AuthorTourDto>($"/api/exploration/tours/mine/{seeded.Id}", JsonOptions);

        tour!.Id.Should().Be(seeded.Id);
        tour.Length!.Kilometers.Should().Be(seeded.Length!.Kilometers);
        tour.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(seeded.KeyPoints.Select(keyPoint => keyPoint.Id));
        tour.KeyPoints.Select(keyPoint => keyPoint.Order).Should().Equal(seeded.KeyPoints.Select(keyPoint => keyPoint.Order));
        tour.KeyPoints.Select(keyPoint => keyPoint.Secret).Should().Equal(seeded.KeyPoints.Select(keyPoint => keyPoint.Secret));
        tour.KeyPoints.Select(keyPoint => keyPoint.Image).Should().Equal(seeded.KeyPoints.Select(keyPoint => keyPoint.Image));
    }

    [Fact]
    public async Task Explorer_does_not_find_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/mine/{TourSeed.PublishableRiverside.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
