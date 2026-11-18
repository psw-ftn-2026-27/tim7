using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPriceTests
{
    [Fact]
    public void Paid_price_with_the_smallest_whole_amount_keeps_that_amount()
    {
        var price = new TourPrice(false, 1m);

        price.IsFree.Should().BeFalse();
        price.Amount.Should().Be(1);
    }

    [Theory]
    [InlineData(49.5)]
    [InlineData(49.9999)]
    public void Paid_price_with_an_amount_that_is_not_whole_is_rejected_instead_of_rounded(double amount)
    {
        var creation = () => new TourPrice(false, (decimal)amount);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Paid_price_with_an_amount_above_the_whole_number_range_is_rejected_as_an_invalid_price()
    {
        var creation = () => new TourPrice(false, (decimal)int.MaxValue + 1);

        creation.Should().Throw<DomainException>();
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
