using System.Text.Json;
using FluentAssertions;
using Games.Application.LocationChallengePlaying;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengePlaying;

public class LocationChallengePlayingQueryTests : BaseIntegrationTest
{
    private const string Route = "/api/games/location-challenges/playing";

    public LocationChallengePlayingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Challenge_view_does_not_contain_the_locations_of_undiscovered_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.CityCenterHunt;

        var response = await client.GetAsync($"{Route}/{challenge.Id}");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContainEquivalentOf("latitude");
        body.Should().NotContainEquivalentOf("longitude");
        var view = JsonSerializer.Deserialize<PlayedLocationChallengeDto>(body, JsonOptions);
        view!.HiddenPoints.Select(point => point.Id).Should()
            .BeEquivalentTo(challenge.HiddenPoints.Select(point => point.Id));
        view.HiddenPoints.Should().OnlyContain(point => !point.IsDiscovered);
    }
}
