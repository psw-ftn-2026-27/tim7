using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/challenges";

    public SocialChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static CreateSocialChallengeDto ValidCreateRequest() =>
        new("Podnevno druženje", new TimeOnly(12, 0), new TimeOnly(14, 0), 8, 6, 75, "Hvala što ste došli na podnevno druženje!");

    private static UpdateSocialChallengeDto ValidUpdateRequest() =>
        new("Ispravljeno druženje", new TimeOnly(9, 0), new TimeOnly(11, 0), 6, 4, 60, "Ispravljena poruka za izlazak iz izazova.");

    [Fact]
    public async Task Administrator_creates_a_draft_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = ValidCreateRequest();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == createdId);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.ActiveWindow.Start.Should().Be(request.Start);
        stored.ActiveWindow.End.Should().Be(request.End);
        stored.Gathering.People.Should().Be(request.People);
        stored.Gathering.RangeMeters.Should().Be(request.RangeMeters);
        stored.Xp.Should().Be(request.Xp);
        stored.ExitMessage.Should().Be(request.ExitMessage);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_challenge_with_invalid_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = ValidCreateRequest() with { Start = new TimeOnly(14, 0), End = new TimeOnly(12, 0) };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(Route, ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync(Route, ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_updates_a_draft_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = ValidUpdateRequest();

        var response = await client.PutAsJsonAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftEveningWalk.Id);
        stored.Name.Should().Be(request.Name);
        stored.ActiveWindow.Start.Should().Be(request.Start);
        stored.ActiveWindow.End.Should().Be(request.End);
        stored.Gathering.People.Should().Be(request.People);
        stored.Gathering.RangeMeters.Should().Be(request.RangeMeters);
        stored.Xp.Should().Be(request.Xp);
        stored.ExitMessage.Should().Be(request.ExitMessage);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_challenge_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", ValidUpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_challenge_with_invalid_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = ValidUpdateRequest() with { Xp = 0 };

        var response = await client.PutAsJsonAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftEveningWalk.Id);
        stored.Name.Should().Be(SocialChallengeSeed.DraftEveningWalk.Name);
        stored.Xp.Should().Be(SocialChallengeSeed.DraftEveningWalk.Xp);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}", ValidUpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_update_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"{Route}/{SocialChallengeSeed.DraftEveningWalk.Id}", ValidUpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"{Route}/{SocialChallengeSeed.DraftMorningMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Any(challenge => challenge.Id == SocialChallengeSeed.DraftMorningMeetup.Id)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_challenge_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"{Route}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"{Route}/{SocialChallengeSeed.DraftMorningMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"{Route}/{SocialChallengeSeed.DraftMorningMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
