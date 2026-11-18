using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInvitations;

public class ClubInvitationsCommandTests : BaseIntegrationTest
{
    public ClubInvitationsCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Invitation_is_stored_for_the_invited_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/invitations/{DemoUsers.Explorer1}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.Invitations.Should().ContainSingle(invitation => invitation.UserId == DemoUsers.Explorer1);
    }

    [Fact]
    public async Task Invitation_of_a_user_who_does_not_exist_is_not_found_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id)
            .Invitations.Count;

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/invitations/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.Invitations.Should().HaveCount(invitationCountBefore);
    }

    [Fact]
    public async Task Invitation_of_a_member_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.WalkersWithDemoMember.Id)
            .Invitations.Count;

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.WalkersWithDemoMember.Id}/invitations/{DemoUsers.Explorer1}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.WalkersWithDemoMember.Id);
        stored.Invitations.Should().HaveCount(invitationCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_invite()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/invitations/{DemoUsers.Explorer1}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_invite()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/invitations/{DemoUsers.Explorer1}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accepted_invitation_stores_the_user_as_a_regular_member_and_removes_their_invitation_and_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.PaintersWithInvitationsAndPendingRequest.Id);
        stored.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.SecondExplorer && member.Role == ClubRole.Member);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Accepting_without_an_invitation_is_not_found_and_the_user_does_not_become_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Rejected_invitation_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.PaintersWithInvitationsAndPendingRequest.Id);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.PaintersWithInvitationsAndPendingRequest.Id}/invitations/mine/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
