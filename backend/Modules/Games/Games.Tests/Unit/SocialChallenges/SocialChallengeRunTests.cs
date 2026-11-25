using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeRunTests
{
    private const int RequiredPeople = 2;
    private const int ChallengeXp = 100;

    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();
    private static readonly Guid Third = Guid.NewGuid();

    private static SocialChallengeRun NewRunOfThree()
    {
        var run = new SocialChallengeRun(Guid.NewGuid(), First);
        run.Join(Second, false);
        run.Join(Third, false);
        return run;
    }

    private static Participant ParticipantOf(SocialChallengeRun run, Guid userId) =>
        run.Participants.Single(participant => participant.UserId == userId);

    [Fact]
    public void Explorer_who_solved_the_challenge_before_cannot_join_the_run()
    {
        var run = new SocialChallengeRun(Guid.NewGuid(), First);

        var joining = () => run.Join(Second, true);

        joining.Should().Throw<DomainException>();
        run.Participants.Should().NotContain(participant => participant.UserId == Second);
    }

    [Fact]
    public void Participant_out_of_range_is_not_counted_as_active()
    {
        var run = NewRunOfThree();
        run.ReportPosition(First, false, Now, RequiredPeople, ChallengeXp);

        run.ReportPosition(Second, true, Now, RequiredPeople, ChallengeXp);

        run.CompletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(31, false)]
    [InlineData(30, true)]
    public void Participant_counts_as_active_only_while_their_last_check_is_at_most_30_seconds_old(int secondsAgo, bool solved)
    {
        var run = NewRunOfThree();
        run.ReportPosition(First, true, Now.AddSeconds(-secondsAgo), RequiredPeople, ChallengeXp);

        run.ReportPosition(Second, true, Now, RequiredPeople, ChallengeXp);

        (run.CompletedAt is not null).Should().Be(solved);
    }

    [Fact]
    public void Enough_active_participants_solve_the_challenge_and_only_the_active_ones_get_the_full_xp()
    {
        var run = NewRunOfThree();
        run.ReportPosition(First, true, Now.AddSeconds(-10), RequiredPeople, ChallengeXp);
        run.ReportPosition(Third, false, Now.AddSeconds(-5), RequiredPeople, ChallengeXp);

        run.ReportPosition(Second, true, Now, RequiredPeople, ChallengeXp);

        run.CompletedAt.Should().Be(Now);
        ParticipantOf(run, First).SolvedAt.Should().Be(Now);
        ParticipantOf(run, First).EarnedXp.Should().Be(ChallengeXp);
        ParticipantOf(run, Second).SolvedAt.Should().Be(Now);
        ParticipantOf(run, Second).EarnedXp.Should().Be(ChallengeXp);
        ParticipantOf(run, Third).SolvedAt.Should().BeNull();
        ParticipantOf(run, Third).EarnedXp.Should().BeNull();
    }
}
