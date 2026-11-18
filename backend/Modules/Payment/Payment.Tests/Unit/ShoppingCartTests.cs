using FluentAssertions;
using Payment.Domain.Carts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public class ShoppingCartTests
{
    [Fact]
    public void Item_that_is_already_in_the_cart_is_rejected()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var entityId = Guid.NewGuid();
        cart.AddItem(entityId, "Tour", 10, Guid.NewGuid());

        var addAgain = () => cart.AddItem(entityId, "Tour", 10, Guid.NewGuid());

        addAgain.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void Item_owned_by_the_buyer_is_rejected()
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);

        var add = () => cart.AddItem(Guid.NewGuid(), "Tour", 10, userId);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Total_price_follows_the_items_added_and_removed()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var removedEntityId = Guid.NewGuid();
        cart.AddItem(Guid.NewGuid(), "First", 10, Guid.NewGuid());
        cart.AddItem(removedEntityId, "Second", 25, Guid.NewGuid());

        cart.RemoveItem(removedEntityId);

        cart.TotalPrice().Should().Be(10);
    }
}
