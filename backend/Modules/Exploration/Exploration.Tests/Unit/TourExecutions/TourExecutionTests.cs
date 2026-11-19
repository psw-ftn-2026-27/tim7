using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionTests
{
    private static TourExecution CreateActiveExecution()
    {
        return new TourExecution(WellKnownUsers.Explorer, Guid.NewGuid(), new Location(45.25, 19.85));
    }

    private static TourExecution CreateAbandonedExecution()
    {
        var execution = CreateActiveExecution();
        execution.Abandon();
        return execution;
    }

    [Fact]
    public void A_session_that_is_not_active_cannot_be_abandoned()
    {
        var execution = CreateAbandonedExecution();

        var abandon = () => execution.Abandon();

        abandon.Should().Throw<DomainException>();
    }

    [Fact]
    public void Completing_the_last_key_point_completes_the_session_and_records_the_time_it_ended()
    {
        var execution = CreateActiveExecution();
        execution.CompleteKeyPoint(Guid.NewGuid(), 2);

        execution.CompleteKeyPoint(Guid.NewGuid(), 2);

        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void A_session_that_is_not_active_does_not_accept_a_position()
    {
        var execution = CreateAbandonedExecution();

        var recordPosition = () => execution.RecordPosition(new Location(45.26, 19.86));

        recordPosition.Should().Throw<DomainException>();
    }
}
