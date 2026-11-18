using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipCommandTests : BaseIntegrationTest
{
    public ClubMembershipCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Request_to_join_is_stored_for_the_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.FruskaGoraHikers.Id);
        stored.JoinRequests.Should().ContainSingle(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Second_request_of_the_same_user_is_rejected_and_no_new_request_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id)
            .JoinRequests.Count;

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.JoinRequests.Should().HaveCount(requestCountBefore);
    }

    [Fact]
    public async Task Request_to_join_a_club_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Clubs.ToList().Sum(club => club.JoinRequests.Count);

        var response = await client.PostAsync($"/api/social/clubs/{Guid.NewGuid()}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.ToList().Sum(club => club.JoinRequests.Count).Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_to_join_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_to_join_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.FruskaGoraHikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Withdrawn_request_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accepted_user_is_stored_as_a_regular_member_and_the_request_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.SecondExplorer && member.Role == ClubRole.Member);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Regular_member_cannot_accept_a_request_and_the_request_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.Administrator}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id);
        stored.JoinRequests.Should().Contain(request => request.UserId == WellKnownUsers.Administrator);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Rejected_request_is_removed_and_the_user_does_not_become_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Regular_member_cannot_reject_a_request_and_the_request_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.Administrator}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id);
        stored.JoinRequests.Should().Contain(request => request.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/join-requests/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
