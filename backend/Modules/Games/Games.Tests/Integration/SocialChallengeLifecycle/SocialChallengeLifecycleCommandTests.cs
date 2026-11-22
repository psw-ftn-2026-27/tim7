using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeLifecycle;

public class SocialChallengeLifecycleCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/challenges";

    public SocialChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(SocialChallengeSeed.DraftEveningWalk.Id).Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Administrator_retires_a_published_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.PublishedNoonPicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(SocialChallengeSeed.PublishedNoonPicnic.Id).Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public async Task Administrator_restores_a_retired_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.RetiredSunsetGathering.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(SocialChallengeSeed.RetiredSunsetGathering.Id).Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Administrator_archives_a_published_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.PublishedNoonPicnic.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(SocialChallengeSeed.PublishedNoonPicnic.Id).Should().Be(SocialChallengeStatus.Archived);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("restore")]
    [InlineData("archive")]
    public async Task Administrator_cannot_change_the_status_of_an_archived_challenge(string action)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.ArchivedWinterWalk.Id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        StoredStatus(SocialChallengeSeed.ArchivedWinterWalk.Id).Should().Be(SocialChallengeStatus.Archived);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("restore")]
    [InlineData("archive")]
    public async Task Administrator_cannot_change_the_status_of_a_challenge_that_does_not_exist(string action)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{Route}/{Guid.NewGuid()}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("restore")]
    [InlineData("archive")]
    public async Task Anonymous_user_cannot_change_the_status_of_a_challenge(string action)
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("restore")]
    [InlineData("archive")]
    public async Task Explorer_cannot_change_the_status_of_a_challenge(string action)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private SocialChallengeStatus StoredStatus(Guid id)
    {
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        return assertContext.SocialChallenges.Single(challenge => challenge.Id == id).Status;
    }
}
