using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubManagement;

public class ClubManagementQueryTests : BaseIntegrationTest
{
    public ClubManagementQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_lists_only_their_own_clubs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<List<ClubDto>>("/api/social/clubs/mine", JsonOptions);

        clubs.Should().HaveCount(ClubSeed.All.Count(club => club.OwnerId == WellKnownUsers.Explorer));
        clubs.Should().OnlyContain(club => club.OwnerId == WellKnownUsers.Explorer);
        clubs.Should().NotContain(club => club.Id == ClubSeed.SecondExplorersCyclists.Id);
    }

    [Fact]
    public async Task Owner_cannot_load_another_owners_club_for_editing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/clubs/mine/{ClubSeed.SecondExplorersCyclists.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
