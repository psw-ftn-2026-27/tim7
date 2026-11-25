using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private const string ValidMessage = "Hvala što ste se okupili na keju!";
    private static readonly Location Quay = new(45.2510, 19.8550);

    private static SocialChallenge CreateChallenge() =>
        new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, ValidMessage, Quay);

    private static SocialChallenge CreateChallengeWithoutLocation() =>
        new("Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, ValidMessage, null);

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
        challenge.Location.Should().Be(Quay);
    }

    [Theory]
    [InlineData(20, 18)]
    [InlineData(18, 18)]
    public void Challenge_cannot_start_after_its_end_or_together_with_it(int startHour, int endHour)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(startHour, 0), new TimeOnly(endHour, 0), 10, 10, 100, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    public void Challenge_requires_from_2_to_100_people(int people)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), people, 50, 100, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(51)]
    public void Challenge_range_must_be_from_2_to_50_meters(int rangeMeters)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 2, rangeMeters, 100, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(13, 2)]
    [InlineData(29, 3)]
    [InlineData(100, 5)]
    public void Challenge_range_must_be_sufficient_for_the_number_of_people(int people, int rangeMeters)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), people, rangeMeters, 100, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Challenge_xp_must_be_positive(int xp)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, xp, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ovaj naziv izazova je predugačak jer ima više od pedeset karaktera")]
    public void Challenge_requires_a_name_of_at_most_50_characters(string name)
    {
        var creation = () => new SocialChallenge(
            name, new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, ValidMessage, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(19)]
    [InlineData(201)]
    public void Challenge_exit_message_has_from_20_to_200_characters(int length)
    {
        var message = new string('a', length);

        var creation = () => new SocialChallenge(
            "Večernja šetnja", new TimeOnly(18, 0), new TimeOnly(20, 0), 10, 10, 100, message, Quay);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_challenge_is_updated()
    {
        var challenge = CreateChallenge();

        challenge.Update("Jutarnja šetnja", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 50, "Vidimo se sutra ujutru opet!", new Location(45.2551, 19.8451));

        challenge.Name.Should().Be("Jutarnja šetnja");
        challenge.ActiveWindow.Start.Should().Be(new TimeOnly(7, 0));
        challenge.ActiveWindow.End.Should().Be(new TimeOnly(9, 0));
        challenge.Gathering.People.Should().Be(4);
        challenge.Gathering.RangeMeters.Should().Be(5);
        challenge.Xp.Should().Be(50);
        challenge.ExitMessage.Should().Be("Vidimo se sutra ujutru opet!");
        challenge.Location.Should().Be(new Location(45.2551, 19.8451));
    }

    [Fact]
    public void Draft_challenge_rejects_invalid_data_on_update()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Jutarnja šetnja", new TimeOnly(9, 0), new TimeOnly(7, 0), 4, 5, 50, ValidMessage, Quay);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_without_a_location_is_not_published()
    {
        var challenge = CreateChallengeWithoutLocation();

        var publication = () => challenge.Publish();

        publication.Should().Throw<DomainException>();
        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public void Challenge_with_a_location_cannot_be_updated_without_one()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Jutarnja šetnja", new TimeOnly(7, 0), new TimeOnly(9, 0), 4, 5, 50, ValidMessage, null);

        update.Should().Throw<DomainException>();
        challenge.Location.Should().Be(Quay);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft, "publish", SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Published, "retire", SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Retired, "restore", SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Published, "archive", SocialChallengeStatus.Archived)]
    [InlineData(SocialChallengeStatus.Retired, "archive", SocialChallengeStatus.Archived)]
    public void Challenge_moves_to_the_requested_status_when_the_transition_is_allowed(
        SocialChallengeStatus from, string action, SocialChallengeStatus expected)
    {
        var challenge = CreateChallengeIn(from);

        Apply(challenge, action);

        challenge.Status.Should().Be(expected);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published, "publish")]
    [InlineData(SocialChallengeStatus.Retired, "publish")]
    [InlineData(SocialChallengeStatus.Archived, "publish")]
    [InlineData(SocialChallengeStatus.Draft, "retire")]
    [InlineData(SocialChallengeStatus.Retired, "retire")]
    [InlineData(SocialChallengeStatus.Archived, "retire")]
    [InlineData(SocialChallengeStatus.Draft, "restore")]
    [InlineData(SocialChallengeStatus.Published, "restore")]
    [InlineData(SocialChallengeStatus.Archived, "restore")]
    [InlineData(SocialChallengeStatus.Draft, "archive")]
    [InlineData(SocialChallengeStatus.Archived, "archive")]
    public void Challenge_rejects_a_disallowed_transition_and_keeps_its_status(
        SocialChallengeStatus from, string action)
    {
        var challenge = CreateChallengeIn(from);

        var transition = () => Apply(challenge, action);

        transition.Should().Throw<DomainException>();
        challenge.Status.Should().Be(from);
    }

    private static SocialChallenge CreateChallengeIn(SocialChallengeStatus status)
    {
        var challenge = CreateChallenge();
        if (status != SocialChallengeStatus.Draft)
        {
            challenge.Publish();
        }
        if (status == SocialChallengeStatus.Retired)
        {
            challenge.Retire();
        }
        if (status == SocialChallengeStatus.Archived)
        {
            challenge.Archive();
        }
        return challenge;
    }

    private static void Apply(SocialChallenge challenge, string action)
    {
        switch (action)
        {
            case "publish": challenge.Publish(); break;
            case "retire": challenge.Retire(); break;
            case "restore": challenge.Restore(); break;
            case "archive": challenge.Archive(); break;
            default: throw new ArgumentException(action);
        }
    }

    [Fact]
    public void Draft_challenge_may_be_deleted()
    {
        var challenge = CreateChallenge();

        var deletion = () => challenge.EnsureDeletable();

        deletion.Should().NotThrow();
    }

    [Fact]
    public void Challenge_that_is_not_published_cannot_be_activated()
    {
        var challenge = CreateChallenge();

        var activation = () => challenge.EnsureCanBeActivatedFrom(Quay, new TimeOnly(19, 0));

        activation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(17, 59)]
    [InlineData(20, 1)]
    public void Published_challenge_cannot_be_activated_outside_its_active_window(int hour, int minute)
    {
        var challenge = CreateChallenge();
        challenge.Publish();

        var activation = () => challenge.EnsureCanBeActivatedFrom(Quay, new TimeOnly(hour, minute));

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_challenge_cannot_be_activated_from_outside_its_range()
    {
        var challenge = CreateChallenge();
        challenge.Publish();
        var positionAbout20MetersNorth = new Location(Quay.Latitude + 0.00018, Quay.Longitude);

        var activation = () => challenge.EnsureCanBeActivatedFrom(positionAbout20MetersNorth, new TimeOnly(19, 0));

        activation.Should().Throw<DomainException>();
    }
}
