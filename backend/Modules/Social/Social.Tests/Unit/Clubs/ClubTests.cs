using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

    [Theory]
    [InlineData("", "Vikend ture po stazama Fruške gore.")]
    [InlineData("   ", "Vikend ture po stazama Fruške gore.")]
    [InlineData("Planinari Fruške gore", "")]
    [InlineData("Planinari Fruške gore", "   ")]
    public void New_club_requires_a_name_and_a_description(string name, string description)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, name, description, ["https://example.com/fruska-gora.jpg"]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_requires_at_least_one_image()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", []);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void Change_that_leaves_the_club_without_images_is_rejected_and_the_club_stays_unchanged()
    {
        var club = CreateClub();

        var update = () => club.Update("Novo ime", "Novi opis.", []);

        update.Should().Throw<DomainException>();
        club.Name.Should().Be("Planinari Fruške gore");
        club.Description.Should().Be("Vikend ture po stazama Fruške gore.");
        club.Images.Should().Equal("https://example.com/fruska-gora.jpg");
    }

    [Fact]
    public void Change_may_remove_the_last_image_while_adding_a_new_one()
    {
        var club = CreateClub();

        club.Update("Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/nova.jpg"]);

        club.Images.Should().Equal("https://example.com/nova.jpg");
    }
}
