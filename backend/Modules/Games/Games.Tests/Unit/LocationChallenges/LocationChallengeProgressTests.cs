using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeProgressTests
{
    private static LocationChallengeProgress NewProgress() => new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Completed_challenge_cannot_be_activated_again()
    {
        var progress = NewProgress();
        progress.Discover(Guid.NewGuid(), 1, 100);

        var activate = () => progress.EnsureCanActivate();

        activate.Should().Throw<DomainException>();
    }

    [Fact]
    public void Same_point_is_recorded_only_once()
    {
        var progress = NewProgress();
        var pointId = Guid.NewGuid();
        progress.Discover(pointId, 2, 100);

        progress.Discover(pointId, 2, 100);

        progress.DiscoveredPointIds.Should().Equal(pointId);
        progress.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Discovering_the_last_point_completes_the_challenge_and_records_the_xp()
    {
        var progress = NewProgress();
        progress.Discover(Guid.NewGuid(), 2, 100);

        progress.Discover(Guid.NewGuid(), 2, 100);

        progress.IsCompleted.Should().BeTrue();
        progress.CompletedAt.Should().NotBeNull();
        progress.EarnedXp.Should().Be(100);
    }
}
