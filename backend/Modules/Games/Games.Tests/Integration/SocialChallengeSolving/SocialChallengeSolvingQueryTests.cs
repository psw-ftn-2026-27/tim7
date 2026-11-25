using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingQueryTests : BaseIntegrationTest
{
    private const string Route = "/api/games/social-challenges/solving";

    public SocialChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Solved_challenge_shows_its_exit_message_and_the_earned_xp()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedAllDayConcert;
        var participant = SocialChallengeRunSeed.ExplorerAndSecondExplorerSolvedAllDayConcert.Participants
            .Single(item => item.UserId == WellKnownUsers.Explorer);

        var view = await client.GetFromJsonAsync<SocialChallengeRunDto>($"{Route}/{challenge.Id}", JsonOptions);

        view!.Progress.Should().BeNull();
        view.Result!.ExitMessage.Should().Be(challenge.ExitMessage);
        view.Result.EarnedXp.Should().Be(participant.EarnedXp);
    }
}
