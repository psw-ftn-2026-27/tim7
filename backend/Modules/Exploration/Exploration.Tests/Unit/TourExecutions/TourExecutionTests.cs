using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionTests
{
    private static TourExecution CreateAbandonedExecution()
    {
        var execution = new TourExecution(WellKnownUsers.Explorer, Guid.NewGuid(), new Location(45.25, 19.85));
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
}
