using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPriceTests
{
    [Fact]
    public void Paid_price_with_the_smallest_positive_amount_is_valid()
    {
        var price = new TourPrice(false, 0.01m);

        price.IsFree.Should().BeFalse();
        price.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Price_without_a_decision_whether_the_tour_is_free_is_rejected()
    {
        var creation = () => new TourPrice(null, null);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Paid_price_requires_an_amount_above_zero(int? amount)
    {
        var creation = () => new TourPrice(false, amount);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_price_cannot_have_an_amount()
    {
        var creation = () => new TourPrice(true, 10m);

        creation.Should().Throw<DomainException>();
    }
}
