using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingCommandTests : BaseIntegrationTest
{
    public TourBrowsingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Location_search_is_recorded_with_its_center_radius_other_criteria_total_result_count_and_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecordLocationSearchDto(new LocationDto(45.2551, 19.8451), 3.5, "staza", [TourDifficulty.Easy, TourDifficulty.Hard], ["priroda"], TransportMode.Walking, 10, 200, 1.5m, 12.5m, 4);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var searchCountBefore = arrangeContext.LocationSearches.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.LocationSearches.Count().Should().Be(searchCountBefore + 1);
        var stored = assertContext.LocationSearches.OrderByDescending(search => search.SearchedAt).First();
        stored.Center.Should().Be(new Location(45.2551, 19.8451));
        stored.Radius.Kilometers.Should().Be(3.5);
        stored.Name.Should().Be("staza");
        stored.Difficulties.Should().Equal(TourDifficulty.Easy, TourDifficulty.Hard);
        stored.Tags.Should().Equal("priroda");
        stored.Transport.Should().Be(TransportMode.Walking);
        stored.MinPrice.Should().Be(10);
        stored.MaxPrice.Should().Be(200);
        stored.MinLength.Should().Be(1.5m);
        stored.MaxLength.Should().Be(12.5m);
        stored.ResultCount.Should().Be(4);
        stored.SearchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Location_search_with_a_zero_radius_is_rejected_and_nothing_is_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecordLocationSearchDto(new LocationDto(45.2551, 19.8451), 0, null, null, null, null, null, null, null, null, 4);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var searchCountBefore = arrangeContext.LocationSearches.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.LocationSearches.Count().Should().Be(searchCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_record_a_location_search()
    {
        var client = Factory.CreateClient();
        var request = new RecordLocationSearchDto(new LocationDto(45.2551, 19.8451), 3.5, null, null, null, null, null, null, null, null, 4);

        var response = await client.PostAsJsonAsync("/api/exploration/tours/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
