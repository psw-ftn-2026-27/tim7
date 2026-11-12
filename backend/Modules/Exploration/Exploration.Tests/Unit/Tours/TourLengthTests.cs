using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourLengthTests
{
    [Theory]
    [InlineData(7)]
    [InlineData(7.5)]
    public void Length_is_a_positive_number_of_kilometers_with_at_most_one_decimal(double kilometers)
    {
        var length = new TourLength((decimal)kilometers);

        length.Kilometers.Should().Be((decimal)kilometers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(7.55)]
    public void Length_is_rejected_when_not_positive_or_with_more_than_one_decimal(double kilometers)
    {
        var creation = () => new TourLength((decimal)kilometers);

        creation.Should().Throw<DomainException>();
    }
}
