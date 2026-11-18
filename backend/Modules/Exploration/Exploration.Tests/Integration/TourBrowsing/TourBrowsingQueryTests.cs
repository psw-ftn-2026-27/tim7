using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        var seededGroupSize = TourSeed.PublishedVineyards.RecommendedGroupSize;
        tours.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id)
            .Which.RecommendedGroupSize.Should().Be(new RecommendedGroupSizeDto(seededGroupSize.From, seededGroupSize.To));
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Tour_is_found_by_part_of_its_name_regardless_of_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=SREMSKIH", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tour_is_listed_when_its_difficulty_is_one_of_the_chosen_difficulties()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Hard", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tour_is_listed_when_it_has_at_least_one_of_the_chosen_tags()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=vino&tags=grad", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedBikeRideToPetrovaradin.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
    }

    [Fact]
    public async Task Tour_is_found_by_a_tag_regardless_of_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=VINO", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tour_is_listed_for_a_transport_mode_only_when_it_has_a_time_for_that_mode()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?transport=Walking", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedBikeRideToPetrovaradin.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tour_that_misses_one_criterion_is_not_listed_even_when_it_meets_the_others()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&tags=istorija&transport=Bicycle", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedBikeRideToPetrovaradin.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
    }

    [Fact]
    public async Task Tours_sorted_by_ascending_difficulty_go_from_easy_through_moderate_to_hard()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sortBy=DifficultyAscending&pageSize=100", JsonOptions);

        tours!.Items.Should().HaveCount(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        tours.Items.Should().BeInAscendingOrder(tour => tour.Difficulty);
    }

    [Fact]
    public async Task Tours_sorted_by_descending_difficulty_go_from_hard_through_moderate_to_easy()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sortBy=DifficultyDescending&pageSize=100", JsonOptions);

        tours!.Items.Should().HaveCount(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        tours.Items.Should().BeInDescendingOrder(tour => tour.Difficulty);
    }

    [Fact]
    public async Task Tours_sorted_by_ascending_price_go_from_free_to_the_most_expensive_and_equal_prices_from_the_newest_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sortBy=PriceAscending&pageSize=100", JsonOptions);

        tours!.Items.Should().HaveCount(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        tours.Items.Select(tour => tour.Amount ?? 0).Should().BeInAscendingOrder();
        tours.Items.Where(tour => tour.Amount == 50).Should().HaveCount(2)
            .And.BeInDescendingOrder(tour => tour.PublishedAt);
    }

    [Fact]
    public async Task Tours_sorted_by_descending_price_go_from_the_most_expensive_to_free()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sortBy=PriceDescending&pageSize=100", JsonOptions);

        tours!.Items.Should().HaveCount(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        tours.Items.Select(tour => tour.Amount ?? 0).Should().BeInDescendingOrder();
        tours.Items.Where(tour => tour.Amount == 50).Should().HaveCount(2)
            .And.BeInDescendingOrder(tour => tour.PublishedAt);
    }

    [Fact]
    public async Task Tours_without_sorting_go_from_the_newest_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().HaveCount(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
        tours.Items.Should().BeInDescendingOrder(tour => tour.PublishedAt);
    }

    [Fact]
    public async Task Page_is_cut_after_filtering_and_the_total_counts_every_matching_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Moderate&difficulties=Hard&page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle()
            .Which.Difficulty.Should().BeOneOf(TourDifficulty.Moderate, TourDifficulty.Hard);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour =>
            tour.Status == TourStatus.Published
            && (tour.Difficulty == TourDifficulty.Moderate || tour.Difficulty == TourDifficulty.Hard)));
    }

    [Fact]
    public async Task Tour_is_listed_when_its_price_is_within_the_range_including_both_bounds()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?minPrice=50&maxPrice=150", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
    }

    [Fact]
    public async Task Price_range_with_only_one_bound_limits_the_tours_only_on_that_side()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?minPrice=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
    }

    [Fact]
    public async Task Price_range_starting_at_zero_includes_a_free_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?minPrice=0&maxPrice=40", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedOldTownWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_published_tours()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/exploration/tours/published");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_gets_another_authors_published_tour_with_its_basic_data_and_only_its_public_key_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = TourSeed.PublishedVineyards;

        var tour = await client.GetFromJsonAsync<PublishedTourDto>($"/api/exploration/tours/{seeded.Id}", JsonOptions);

        tour!.Id.Should().Be(seeded.Id);
        tour.Name.Should().Be(seeded.Name);
        tour.Description.Should().Be(seeded.Description);
        tour.Difficulty.Should().Be(seeded.Difficulty);
        tour.Tags.Should().Equal(seeded.Tags);
        tour.Length!.Kilometers.Should().Be(seeded.Length!.Kilometers);
        tour.TransportTimes.Should().Equal(seeded.TransportTimes.Select(time => new TransportTimeDto(time.Transport, time.Minutes)));
        tour.IsFree.Should().Be(seeded.Price!.IsFree);
        tour.KeyPoints.Should().Equal(seeded.KeyPoints
            .Where(keyPoint => keyPoint.IsPublic)
            .Select(keyPoint => new PublicKeyPointDto(
                keyPoint.Id,
                keyPoint.Name,
                keyPoint.Description,
                keyPoint.Image,
                new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude))));
    }

    [Fact]
    public async Task Public_key_point_of_a_published_tour_arrives_without_its_secret()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = TourSeed.PublishedVineyards;
        var publicKeyPoint = seeded.KeyPoints.First(keyPoint => keyPoint.IsPublic && keyPoint.Secret is not null);

        var body = await client.GetStringAsync($"/api/exploration/tours/{seeded.Id}");

        body.Should().Contain(publicKeyPoint.Id.ToString());
        body.Should().NotContain(publicKeyPoint.Secret!);
    }

    public static TheoryData<Guid> ToursThatAreNotPublished => new()
    {
        TourSeed.FortressWalk.Id,
        TourSeed.ArchivedLakeside.Id,
        Guid.NewGuid()
    };

    [Theory]
    [MemberData(nameof(ToursThatAreNotPublished))]
    public async Task Tour_in_preparation_archived_tour_and_missing_tour_are_not_found(Guid tourId)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{tourId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_view_a_published_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
