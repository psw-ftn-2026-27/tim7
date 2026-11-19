using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringQueryTests : BaseIntegrationTest
{
    private const string Route = "/api/games/location-challenges";

    public LocationChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_lists_all_location_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.CityCenterHunt;

        var challenges = await client.GetFromJsonAsync<List<LocationChallengeDto>>(Route, JsonOptions);

        challenges!.Select(challenge => challenge.Id).Should()
            .BeEquivalentTo(LocationChallengeSeed.All.Select(challenge => challenge.Id));
        var listed = challenges!.Single(challenge => challenge.Id == seeded.Id);
        listed.Name.Should().Be(seeded.Name);
        listed.StartLocation.Latitude.Should().Be(seeded.StartLocation.Latitude);
        listed.StartLocation.Longitude.Should().Be(seeded.StartLocation.Longitude);
        listed.Xp.Should().Be(seeded.Xp);
        listed.HiddenPointCount.Should().Be(seeded.HiddenPoints.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_location_challenges()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_list_location_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
