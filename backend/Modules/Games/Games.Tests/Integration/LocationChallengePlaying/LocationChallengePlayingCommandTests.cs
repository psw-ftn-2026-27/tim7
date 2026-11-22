using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengePlaying;

public class LocationChallengePlayingCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/location-challenges/playing";

    public LocationChallengePlayingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_checks_the_location_near_an_undiscovered_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.CityCenterHunt;
        var point = challenge.HiddenPoints[0];
        var position = new LocationDto(point.Location.Latitude, point.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/check", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var progress = assertContext.LocationChallengeProgresses
            .Single(stored => stored.Id == LocationChallengeProgressSeed.ExplorerInCityCenterHunt.Id);
        progress.DiscoveredPointIds.Should().Equal(point.Id);
    }

    [Fact]
    public async Task Explorer_cannot_check_a_location_that_is_not_near_an_undiscovered_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.CityCenterHunt;
        var position = new LocationDto(44.8005, 20.4600);

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/check", position);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var progress = assertContext.LocationChallengeProgresses
            .Single(stored => stored.Id == LocationChallengeProgressSeed.ExplorerInCityCenterHunt.Id);
        progress.DiscoveredPointIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_location()
    {
        var client = Factory.CreateClient();
        var challenge = LocationChallengeSeed.CityCenterHunt;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/check", new LocationDto(44.801, 20.46));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = LocationChallengeSeed.CityCenterHunt;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/check", new LocationDto(44.801, 20.46));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_activates_a_challenge_from_inside_its_circle()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.RiverbankWalk;
        var position = new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeProgresses.Should().ContainSingle(progress =>
            progress.UserId == WellKnownUsers.Explorer && progress.LocationChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Explorer_cannot_activate_a_challenge_from_outside_its_circle()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.RiverbankWalk;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", new LocationDto(44.90, 20.44));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeProgresses.Should().NotContain(progress =>
            progress.UserId == WellKnownUsers.Explorer && progress.LocationChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_challenge()
    {
        var client = Factory.CreateClient();
        var challenge = LocationChallengeSeed.RiverbankWalk;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", new LocationDto(44.82, 20.44));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = LocationChallengeSeed.RiverbankWalk;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", new LocationDto(44.82, 20.44));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
