using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Core.PositionSimulation;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration.PositionSimulation;

public class PositionSimulationCommandTests : BaseIntegrationTest
{
    public PositionSimulationCommandTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_who_marks_a_position_for_the_first_time_has_that_position_saved()
    {
        var client = Factory.CreateClientFor(UserSeed.LoneExplorer.Id, "explorer");
        using var arrangeContext = Factory.CreateContext<IdentityModuleDbContext>();
        var positionCountBefore = arrangeContext.UserPositions.Count();

        var response = await client.PutAsJsonAsync("/api/identity/position", new SetPositionDto(43.3209, 21.8958), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        assertContext.UserPositions.Count().Should().Be(positionCountBefore + 1);
        var stored = assertContext.UserPositions.Single(position => position.UserId == UserSeed.LoneExplorer.Id);
        stored.Coordinates.Latitude.Should().Be(43.3209);
        stored.Coordinates.Longitude.Should().Be(21.8958);
    }

    [Fact]
    public async Task Explorer_who_already_has_a_position_still_has_one_position_with_the_new_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<IdentityModuleDbContext>();
        var positionCountBefore = arrangeContext.UserPositions.Count();

        var response = await client.PutAsJsonAsync("/api/identity/position", new SetPositionDto(43.3209, 21.8958), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        assertContext.UserPositions.Count().Should().Be(positionCountBefore);
        var stored = assertContext.UserPositions.Single(position => position.UserId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(UserPositionSeed.ExplorerStandsInNoviSad.Id);
        stored.Coordinates.Latitude.Should().Be(43.3209);
        stored.Coordinates.Longitude.Should().Be(21.8958);
    }

    [Fact]
    public async Task Position_out_of_range_is_refused_and_the_earlier_position_stays_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync("/api/identity/position", new SetPositionDto(45.2671, 200.5), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        var stored = assertContext.UserPositions.Single(position => position.UserId == WellKnownUsers.Explorer);
        stored.Coordinates.Should().Be(UserPositionSeed.ExplorerStandsInNoviSad.Coordinates);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/identity/position", new SetPositionDto(43.3209, 21.8958), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
