using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubInvitations;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInvitations;

public class ClubInvitationsQueryTests : BaseIntegrationTest
{
    public ClubInvitationsQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_who_is_not_the_owner_gets_no_search_results()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/explorers?username=explorer");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_leaves_out_members_and_users_already_invited()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var explorers = await client.GetFromJsonAsync<List<ExplorerDto>>(
            $"/api/social/clubs/{ClubSeed.SingersWithDemoMemberAndDemoInvitation.Id}/explorers?username=explorer", JsonOptions);

        explorers.Should().Contain(explorer => explorer.Id == DemoUsers.Explorer3);
        explorers.Should().NotContain(explorer => explorer.Id == DemoUsers.Explorer1);
        explorers.Should().NotContain(explorer => explorer.Id == DemoUsers.Explorer2);
    }

    [Fact]
    public async Task Invited_user_sees_their_invitations_with_the_club_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var invitations = await client.GetFromJsonAsync<List<MyInvitationDto>>(
            "/api/social/clubs/invitations", JsonOptions);

        invitations.Should().Contain(new MyInvitationDto(
            ClubSeed.PaintersWithInvitationsAndPendingRequest.Id,
            ClubSeed.PaintersWithInvitationsAndPendingRequest.Name));
    }
}
