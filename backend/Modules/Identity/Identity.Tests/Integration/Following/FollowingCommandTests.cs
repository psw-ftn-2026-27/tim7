using System.Net;
using FluentAssertions;
using Identity.Core.Connections;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration.Following;

public class FollowingCommandTests : BaseIntegrationTest
{
    public FollowingCommandTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Sent_request_is_recorded_as_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<IdentityModuleDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync($"/api/identity/requests/sent/{UserSeed.LoneExplorer.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore + 1);
        assertContext.Connections.SelectMany(connection => connection.Requests)
            .Should().ContainSingle(request => request.SenderId == WellKnownUsers.Explorer
                && request.RecipientId == UserSeed.LoneExplorer.Id
                && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Second_request_while_the_first_is_pending_is_refused_and_nothing_is_written()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<IdentityModuleDbContext>();
        var requestCountBefore = arrangeContext.Connections.SelectMany(connection => connection.Requests).Count();

        var response = await client.PostAsync($"/api/identity/requests/sent/{UserSeed.AwaitedExplorer.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        assertContext.Connections.SelectMany(connection => connection.Requests).Count()
            .Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/identity/requests/sent/{UserSeed.LoneExplorer.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/identity/requests/sent/{UserSeed.LoneExplorer.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accepted_request_makes_the_sender_a_follower()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.RequestingExplorerAwaitsExplorer.Id);
        stored.Requests.Should().ContainSingle(request => request.SenderId == UserSeed.RequestingExplorer.Id
            && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task Accepting_a_request_from_an_unconnected_user_is_refused_as_missing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<IdentityModuleDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.LoneExplorer.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Rejected_request_stays_recorded_as_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.RequestingExplorerAwaitsExplorer.Id);
        stored.Requests.Should().ContainSingle(request => request.SenderId == UserSeed.RequestingExplorer.Id
            && request.Status == FollowRequestStatus.Rejected);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/identity/requests/received/{UserSeed.RequestingExplorer.Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Withdrawn_request_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/identity/requests/sent/{UserSeed.AwaitedExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<IdentityModuleDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.ExplorerAwaitsAwaitedExplorer.Id);
        stored.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/identity/requests/sent/{UserSeed.AwaitedExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/identity/requests/sent/{UserSeed.AwaitedExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
