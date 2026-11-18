using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class RecommendedGroupSizeTests
{
    [Fact]
    public void Group_size_may_start_and_end_at_the_same_number()
    {
        var groupSize = new RecommendedGroupSize(4, 4);

        groupSize.From.Should().Be(4);
        groupSize.To.Should().Be(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Group_size_requires_at_least_one_person(int from)
    {
        var creation = () => new RecommendedGroupSize(from, 6);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Group_size_cannot_start_above_its_upper_limit()
    {
        var creation = () => new RecommendedGroupSize(6, 2);

        creation.Should().Throw<DomainException>();
    }
}
