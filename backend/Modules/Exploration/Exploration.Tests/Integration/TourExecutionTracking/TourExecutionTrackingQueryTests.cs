using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecutionTracking;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecutionTracking;

public class TourExecutionTrackingQueryTests : BaseIntegrationTest
{
    public TourExecutionTrackingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_tour_shows_the_start_location_and_the_first_key_point_by_order_even_when_it_is_not_public()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var mine = await client.GetFromJsonAsync<MyActiveTourExecutionDto>("/api/exploration/tour-executions/active", JsonOptions);

        var seeded = TourExecutionSeed.ExplorersActiveExecution;
        var tour = TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint;
        var firstKeyPoint = tour.KeyPoints[0];
        mine!.Execution.Should().Be(new ActiveTourExecutionDto(
            seeded.Id,
            tour.Id,
            tour.Name,
            new LocationDto(seeded.StartLocation.Latitude, seeded.StartLocation.Longitude),
            new TourExecutionKeyPointDto(
                firstKeyPoint.Id,
                firstKeyPoint.Name,
                firstKeyPoint.Description,
                firstKeyPoint.Image,
                new LocationDto(firstKeyPoint.Location.Latitude, firstKeyPoint.Location.Longitude))));
    }

    [Fact]
    public async Task Active_session_of_an_archived_tour_is_still_shown()
    {
        var client = Factory.CreateClientFor(TourExecutionSeed.ThirdExplorer, "explorer");

        var mine = await client.GetFromJsonAsync<MyActiveTourExecutionDto>("/api/exploration/tour-executions/active", JsonOptions);

        mine!.Execution!.Id.Should().Be(TourExecutionSeed.ThirdExplorersActiveExecutionOfArchivedTour.Id);
        mine.Execution.TourId.Should().Be(TourSeed.ArchivedLakeside.Id);
    }

    [Fact]
    public async Task Tourist_without_an_active_session_learns_that_no_tour_is_active()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/exploration/tour-executions/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mine = await response.Content.ReadFromJsonAsync<MyActiveTourExecutionDto>(JsonOptions);
        mine!.Execution.Should().BeNull();
    }
}
