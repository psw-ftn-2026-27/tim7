using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubRoles;

public class ClubRolesCommandTests : BaseIntegrationTest
{
    public ClubRolesCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Promoted_member_is_stored_as_a_moderator()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.Administrator && member.Role == ClubRole.Moderator);
    }

    [Fact]
    public async Task Promotion_by_a_user_who_is_not_the_owner_is_rejected_and_the_role_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.Administrator && member.Role == ClubRole.Member);
    }

    [Fact]
    public async Task Promotion_in_a_club_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var moderatorCountBefore = arrangeContext.Clubs.ToList()
            .Sum(club => club.Members.Count(member => member.Role == ClubRole.Moderator));

        var response = await client.PostAsync(
            $"/api/social/clubs/{Guid.NewGuid()}/members/{WellKnownUsers.Administrator}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.ToList()
            .Sum(club => club.Members.Count(member => member.Role == ClubRole.Moderator))
            .Should().Be(moderatorCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_promote_a_member()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_promote_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Transfer_stores_the_new_owner_and_the_old_owner_as_a_regular_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.ClimbersWithExplorerModeratorAndPendingRequest.Id}/members/{WellKnownUsers.Explorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.ClimbersWithExplorerModeratorAndPendingRequest.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.Explorer);
        stored.Members.Should().ContainSingle(member =>
            member.UserId == WellKnownUsers.SecondExplorer && member.Role == ClubRole.Member);
    }

    [Fact]
    public async Task Transfer_by_a_user_who_is_not_the_owner_is_rejected_and_the_owner_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Transfer_in_a_club_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var ownedCountBefore = arrangeContext.Clubs.Count(club => club.OwnerId == WellKnownUsers.Administrator);

        var response = await client.PostAsync(
            $"/api/social/clubs/{Guid.NewGuid()}/members/{WellKnownUsers.Administrator}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count(club => club.OwnerId == WellKnownUsers.Administrator).Should().Be(ownedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_transfer_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_transfer_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Removed_member_is_deleted_from_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.ClimbersWithExplorerModeratorAndPendingRequest.Id}/members/{WellKnownUsers.Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.ClimbersWithExplorerModeratorAndPendingRequest.Id);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Removal_by_a_user_without_rights_is_rejected_and_the_member_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.RunnersWithMemberAndPendingRequest.Id);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Removal_from_a_club_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var memberCountBefore = arrangeContext.Clubs.ToList().Sum(club => club.Members.Count);

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{Guid.NewGuid()}/members/{WellKnownUsers.Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.ToList().Sum(club => club.Members.Count).Should().Be(memberCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_member()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersWithMemberAndPendingRequest.Id}/members/{WellKnownUsers.Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Member_promoted_to_moderator_is_no_longer_muted_in_the_chat()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersWithExplorerMemberAndPendingRequest.Id}/members/{WellKnownUsers.Explorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Chats.Single(chat => chat.Id == ChatSeed.SwimmersChatWithMutedExplorer.Id).MutedUsers
            .Should().BeEmpty();
    }
}
