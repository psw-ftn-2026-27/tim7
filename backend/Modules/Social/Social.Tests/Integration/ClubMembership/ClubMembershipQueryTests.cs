using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubMembership;
using Social.Domain.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipQueryTests : BaseIntegrationTest
{
    public ClubMembershipQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_sees_pending_requests_and_the_members_with_themselves_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}", JsonOptions);

        club!.Members.First().Should().Be(new ClubMemberDto(WellKnownUsers.Explorer, ClubRole.Owner));
        club.Members.Should().Contain(new ClubMemberDto(WellKnownUsers.Administrator, ClubRole.Member));
        club.PendingRequests.Should().Contain(WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task User_with_a_pending_request_sees_that_it_is_pending_and_does_not_get_the_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}", JsonOptions);

        club!.MyState.Should().Be(MembershipState.Pending);
        club.PendingRequests.Should().BeNull();
    }

    [Fact]
    public async Task Moderator_gets_their_role_and_the_pending_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.ClimbersWithExplorerModeratorAndPendingRequest.Id}", JsonOptions);

        club!.MyRole.Should().Be(ClubRole.Moderator);
        club.PendingRequests.Should().Contain(WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Owner_sees_the_sent_invitations_with_the_e_mails_of_the_invited_users()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}", JsonOptions);

        club!.SentInvitations.Should().Contain(new ClubInvitationDto(DemoUsers.Explorer2, "explorer2@explorer.dev"));
        club.SentInvitations.Should().Contain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Page_of_a_closed_club_shows_that_it_is_closed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.ClosedKayakers.Id}", JsonOptions);

        club!.Status.Should().Be(ClubStatus.Closed);
    }
}
