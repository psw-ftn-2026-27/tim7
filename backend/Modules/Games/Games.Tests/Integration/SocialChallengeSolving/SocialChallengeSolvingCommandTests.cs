using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/social-challenges/solving";

    public SocialChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    private static LocationDto PositionAt(SocialChallenge challenge) =>
        new(challenge.Location!.Latitude, challenge.Location.Longitude);

    [Fact]
    public async Task First_explorer_to_activate_a_challenge_starts_its_run()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayStroll;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var run = assertContext.SocialChallengeRuns.Should()
            .ContainSingle(stored => stored.ChallengeId == challenge.Id).Subject;
        run.Participants.Should().ContainSingle(participant => participant.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Next_explorer_to_activate_a_challenge_joins_its_active_run()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayGathering;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var run = assertContext.SocialChallengeRuns.Should()
            .ContainSingle(stored => stored.ChallengeId == challenge.Id).Subject;
        run.Id.Should().Be(SocialChallengeRunSeed.SecondExplorerWaitingAtAllDayGathering.Id);
        run.Participants.Select(participant => participant.UserId).Should()
            .BeEquivalentTo([WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer]);
    }

    [Fact]
    public async Task Explorer_cannot_activate_a_challenge_from_outside_its_range_and_no_run_starts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayStroll;
        var positionAbout500MetersNorth = new LocationDto(challenge.Location!.Latitude + 0.0045, challenge.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", positionAbout500MetersNorth);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Should().NotContain(run => run.ChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var challenge = SocialChallengeSeed.PublishedAllDayStroll;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = SocialChallengeSeed.PublishedAllDayStroll;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/activate", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Position_that_gathers_enough_participants_solves_the_challenge_for_each_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayMeetup;
        var seeded = SocialChallengeRunSeed.ExplorerAndSecondExplorerInRangeAtAllDayMeetup;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/position", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var run = assertContext.SocialChallengeRuns.Single(stored => stored.Id == seeded.Id);
        run.CompletedAt.Should().NotBeNull();
        run.Participants.Should().OnlyContain(participant =>
            participant.SolvedAt != null && participant.EarnedXp == challenge.Xp);
    }

    [Fact]
    public async Task Position_of_an_explorer_who_has_not_joined_the_run_is_rejected_as_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayGathering;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/position", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns
            .Where(run => run.ChallengeId == challenge.Id)
            .SelectMany(run => run.Participants)
            .Should().NotContain(participant => participant.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_position()
    {
        var client = Factory.CreateClient();
        var challenge = SocialChallengeSeed.PublishedAllDayMeetup;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/position", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = SocialChallengeSeed.PublishedAllDayMeetup;

        var response = await client.PostAsJsonAsync($"{Route}/{challenge.Id}/position", PositionAt(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
