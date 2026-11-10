using System.Net.Http.Json;
using FluentAssertions;
using Identity.Core.Connections;
using Identity.Core.Following;
using Identity.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration.Following;

public class FollowingQueryTests : BaseIntegrationTest
{
    public FollowingQueryTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Followers_list_shows_the_seeded_followers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<UserDto>>("/api/identity/followers", JsonOptions);

        followers!.Items.Should().ContainSingle(user =>
            user.Id == UserSeed.SecondExplorer.Id && user.Email == UserSeed.SecondExplorer.Email);
        followers.Items.Should().NotContain(user => user.Id == UserSeed.FollowedExplorer.Id);
        followers.Items.Should().NotContain(user => user.Id == UserSeed.RequestingExplorer.Id);
    }

    [Fact]
    public async Task Received_requests_show_the_seeded_pending_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await client.GetFromJsonAsync<FollowRequestsDto>("/api/identity/requests", JsonOptions);

        requests!.Received.Should().ContainSingle(request =>
            request.UserId == UserSeed.RequestingExplorer.Id
            && request.Email == UserSeed.RequestingExplorer.Email
            && request.Status == FollowRequestStatus.Pending);
        requests.Received.Should().NotContain(request => request.UserId == UserSeed.SecondExplorer.Id);
    }

    [Fact]
    public async Task Search_finds_users_by_part_of_their_email_with_their_relationship_to_the_caller()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var found = await client.GetFromJsonAsync<PageResult<FoundUserDto>>(
            "/api/identity/users?email=EXPLORER", JsonOptions);

        found!.TotalCount.Should().Be(UserSeed.Users.Count(user =>
            user.Id != WellKnownUsers.Explorer && user.Email!.Contains("explorer")));
        found.Items.Should().NotContain(user => user.Id == WellKnownUsers.Explorer);
        found.Items.Should().NotContain(user => user.Id == UserSeed.Administrator.Id);
        found.Items.Should().ContainSingle(user =>
            user.Id == UserSeed.FollowedExplorer.Id && user.Status == RelationshipStatus.Following);
        found.Items.Should().ContainSingle(user =>
            user.Id == UserSeed.AwaitedExplorer.Id && user.Status == RelationshipStatus.Pending);
        found.Items.Should().ContainSingle(user =>
            user.Id == UserSeed.SecondExplorer.Id && user.Status == RelationshipStatus.None);
    }
}
