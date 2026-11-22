using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Core.PositionSimulation;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration.PositionSimulation;

public class PositionSimulationQueryTests : BaseIntegrationTest
{
    public PositionSimulationQueryTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_gets_their_own_position_and_not_the_position_of_another_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var position = await client.GetFromJsonAsync<PositionDto>("/api/identity/position", JsonOptions);

        var own = UserPositionSeed.ExplorerStandsInNoviSad.Coordinates;
        position.Should().Be(new PositionDto(own.Latitude, own.Longitude));
    }

    [Fact]
    public async Task Explorer_without_a_position_learns_that_there_is_none_and_gets_no_error()
    {
        var client = Factory.CreateClientFor(UserSeed.LoneExplorer.Id, "explorer");

        var response = await client.GetAsync("/api/identity/position");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
