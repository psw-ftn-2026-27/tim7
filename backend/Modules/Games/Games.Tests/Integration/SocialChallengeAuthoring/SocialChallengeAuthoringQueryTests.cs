using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringQueryTests : BaseIntegrationTest
{
    private const string Route = "/api/games/challenges";

    public SocialChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_lists_all_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>(Route, JsonOptions);

        challenges!.Select(challenge => challenge.Id).Should()
            .BeEquivalentTo(SocialChallengeSeed.All.Select(challenge => challenge.Id));
    }

    [Fact]
    public async Task Administrator_loads_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = SocialChallengeSeed.DraftEveningWalk;

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>($"{Route}/{seeded.Id}", JsonOptions);

        challenge!.Id.Should().Be(seeded.Id);
        challenge.Name.Should().Be(seeded.Name);
        challenge.Start.Should().Be(seeded.ActiveWindow.Start);
        challenge.End.Should().Be(seeded.ActiveWindow.End);
        challenge.People.Should().Be(seeded.Gathering.People);
        challenge.RangeMeters.Should().Be(seeded.Gathering.RangeMeters);
        challenge.Xp.Should().Be(seeded.Xp);
        challenge.ExitMessage.Should().Be(seeded.ExitMessage);
        challenge.Status.Should().Be(seeded.Status);
    }
}
