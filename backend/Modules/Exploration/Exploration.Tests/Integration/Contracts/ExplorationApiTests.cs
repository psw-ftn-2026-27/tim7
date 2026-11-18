using Exploration.Contracts;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Exploration.Tests.Integration.Contracts;

public class ExplorationApiTests : BaseIntegrationTest
{
    public ExplorationApiTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Sale_data_of_a_published_paid_tour_carries_its_identity_price_and_published_flag()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();
        var tour = TourSeed.PublishedVineyards;

        var forSale = await explorationApi.GetTourForSaleAsync(tour.Id);

        forSale.Should().Be(new TourForSaleDto(tour.Id, tour.Name, tour.AuthorId, tour.Price!.IsFree, tour.Price.Amount, true));
    }

    [Fact]
    public async Task Archived_tour_keeps_its_price_in_sale_data_but_is_not_published()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();
        var tour = TourSeed.ArchivedLakeside;

        var forSale = await explorationApi.GetTourForSaleAsync(tour.Id);

        forSale!.IsFree.Should().Be(tour.Price!.IsFree);
        forSale.Amount.Should().Be(tour.Price.Amount);
        forSale.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Draft_in_sale_data_has_no_free_flag_no_amount_and_is_not_published()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var forSale = await explorationApi.GetTourForSaleAsync(TourSeed.FortressWalk.Id);

        forSale!.IsFree.Should().BeNull();
        forSale.Amount.Should().BeNull();
        forSale.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Sale_data_of_a_tour_that_does_not_exist_is_empty()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();
        var unknownId = Guid.Parse("00000000-0000-0000-0009-000000000001");

        var forSale = await explorationApi.GetTourForSaleAsync(unknownId);

        forSale.Should().BeNull();
    }
}
