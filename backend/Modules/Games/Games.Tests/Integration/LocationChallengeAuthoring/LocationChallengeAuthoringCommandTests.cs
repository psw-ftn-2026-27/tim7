using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/location-challenges";

    public LocationChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var request = CreateForm("Potraga", 120, [0.001, 0.002]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == createdId);
        stored.Name.Should().Be("Potraga");
        stored.Xp.Should().Be(120);
        stored.StartLocation.Latitude.Should().Be(44.8);
        stored.RadiusBonusMeters.Should().BeInRange(0, 100);
        stored.HiddenPoints.Should().HaveCount(2);
        stored.HiddenPoints.Should().OnlyContain(point => point.Image.StartsWith("/images/games/"));
    }

    [Fact]
    public async Task Administrator_cannot_create_a_location_challenge_with_a_point_too_far_from_the_start()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var request = CreateForm("Potraga", 120, [0.001, 0.002, 0.003, 0.0045]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClient();
        using var request = CreateForm("Potraga", 120, [0.001, 0.002]);

        var response = await client.PostAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var request = CreateForm("Potraga", 120, [0.001, 0.002]);

        var response = await client.PostAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static byte[] CreatePng()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .. "IHDR"u8, 0, 0, 0, 1, 0, 0, 0, 1];
        return content;
    }

    // The start location is at latitude 44.8; every hidden point lies the given number of degrees of latitude north of it.
    // The photo at position i travels with the point at position i.
    private static MultipartFormDataContent CreateForm(string name, int xp, double[] pointOffsets)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(xp.ToString(CultureInfo.InvariantCulture)), "Xp" },
            { new StringContent("44.8"), "StartLocation.Latitude" },
            { new StringContent("20.46"), "StartLocation.Longitude" }
        };
        for (var i = 0; i < pointOffsets.Length; i++)
        {
            var latitude = (44.8 + pointOffsets[i]).ToString(CultureInfo.InvariantCulture);
            form.Add(new StringContent(latitude), $"HiddenPoints[{i}].Location.Latitude");
            form.Add(new StringContent("20.46"), $"HiddenPoints[{i}].Location.Longitude");
            form.Add(new ByteArrayContent(CreatePng()), "images", "slika.png");
        }
        return form;
    }
}
