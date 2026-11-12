using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private const string ValidMessage = "Hvala što ste se okupili na keju!";

    private static SocialChallenge CreateChallenge() =>
        new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, ValidMessage);

    [Fact]
    public void New_challenge_starts_as_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
        challenge.Name.Should().Be("Večernja šetnja");
        challenge.ActiveWindow.Start.Should().Be(new TimeOnly(18, 0));
        challenge.ActiveWindow.End.Should().Be(new TimeOnly(20, 0));
        challenge.Gathering.People.Should().Be(10);
        challenge.Gathering.RangeMeters.Should().Be(10);
        challenge.Xp.Should().Be(100);
        challenge.ExitMessage.Should().Be(ValidMessage);
    }

    [Theory]
    [InlineData(20, 18)]
    [InlineData(18, 18)]
    public void Challenge_cannot_start_after_its_end_or_together_with_it(int startHour, int endHour)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(startHour, 0), new TimeOnly(endHour, 0), 10, 10, 100, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    public void Challenge_requires_from_2_to_100_people(int people)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), people, 50, 100, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(51)]
    public void Challenge_range_must_be_from_2_to_50_meters(int rangeMeters)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 2, rangeMeters, 100, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(13, 2)]
    [InlineData(29, 3)]
    [InlineData(100, 5)]
    public void Challenge_range_must_be_sufficient_for_the_number_of_people(int people, int rangeMeters)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), people, rangeMeters, 100, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Challenge_xp_must_be_positive(int xp)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, xp, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ovaj naziv izazova je predugačak jer ima više od pedeset karaktera")]
    public void Challenge_requires_a_name_of_at_most_50_characters(string name)
    {
        var creation = () => new SocialChallenge(
            name, new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, ValidMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(19)]
    [InlineData(201)]
    public void Challenge_exit_message_has_from_20_to_200_characters(int length)
    {
        var message = new string('a', length);

        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, message);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_challenge_is_updated()
    {
        var challenge = CreateChallenge();

        challenge.Update("Jutarnja šetnja", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 50, "Vidimo se sutra ujutru opet!");

        challenge.Name.Should().Be("Jutarnja šetnja");
        challenge.ActiveWindow.Start.Should().Be(new TimeOnly(7, 0));
        challenge.ActiveWindow.End.Should().Be(new TimeOnly(9, 0));
        challenge.Gathering.People.Should().Be(4);
        challenge.Gathering.RangeMeters.Should().Be(5);
        challenge.Xp.Should().Be(50);
        challenge.ExitMessage.Should().Be("Vidimo se sutra ujutru opet!");
    }

    [Fact]
    public void Draft_challenge_rejects_invalid_data_on_update()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Jutarnja šetnja", new TimeOnly(9, 0), new TimeOnly(7, 0), 4, 5, 50, ValidMessage);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_challenge_may_be_deleted()
    {
        var challenge = CreateChallenge();

        var deletion = () => challenge.EnsureDeletable();

        deletion.Should().NotThrow();
    }
}
