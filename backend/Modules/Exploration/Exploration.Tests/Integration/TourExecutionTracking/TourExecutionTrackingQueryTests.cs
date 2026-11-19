using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exploration.Application.TourExecutionTracking;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecutionTracking;

public class TourExecutionTrackingQueryTests : BaseIntegrationTest
{
    // The database keeps times to the microsecond, the seeds in memory to the tick.
    private static readonly TimeSpan StoredTimePrecision = TimeSpan.FromMicroseconds(1);

    public TourExecutionTrackingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_tour_without_progress_shows_the_start_location_and_the_first_key_point_by_order_even_when_it_is_not_public()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var mine = await client.GetFromJsonAsync<MyActiveTourExecutionDto>("/api/exploration/tour-executions/active", JsonOptions);

        var seeded = TourExecutionSeed.ExplorersActiveExecution;
        var tour = TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint;
        var firstKeyPoint = tour.KeyPoints[0];
        mine!.Execution.Should().BeEquivalentTo(new ActiveTourExecutionDto(
            seeded.Id,
            tour.Id,
            tour.Name,
            new LocationDto(seeded.StartLocation.Latitude, seeded.StartLocation.Longitude),
            [],
            new TourExecutionKeyPointDto(
                firstKeyPoint.Id,
                firstKeyPoint.Name,
                firstKeyPoint.Description,
                firstKeyPoint.Image,
                new LocationDto(firstKeyPoint.Location.Latitude, firstKeyPoint.Location.Longitude))));
    }

    [Fact]
    public async Task Active_tour_shows_the_tourists_last_position_the_secrets_of_completed_key_points_and_the_next_key_point_without_its_secret()
    {
        var client = Factory.CreateClientFor(TourExecutionSeed.FourthExplorer, "explorer");

        var body = await client.GetStringAsync("/api/exploration/tour-executions/active");

        var seeded = TourExecutionSeed.FourthExplorersActiveExecutionPastTheFirstKeyPoint;
        var tour = TourSeed.PublishedVineyards;
        var completedKeyPoint = tour.KeyPoints[0];
        var nextKeyPoint = tour.KeyPoints[1];
        var mine = JsonSerializer.Deserialize<MyActiveTourExecutionDto>(body, JsonOptions);
        mine!.Execution.Should().BeEquivalentTo(
            new ActiveTourExecutionDto(
                seeded.Id,
                tour.Id,
                tour.Name,
                new LocationDto(seeded.LastKnownLocation.Latitude, seeded.LastKnownLocation.Longitude),
                [new CompletedKeyPointDto(completedKeyPoint.Id, completedKeyPoint.Order, completedKeyPoint.Name, seeded.KeyPointCompletions[0].CompletedAt, completedKeyPoint.Secret)],
                new TourExecutionKeyPointDto(
                    nextKeyPoint.Id,
                    nextKeyPoint.Name,
                    nextKeyPoint.Description,
                    nextKeyPoint.Image,
                    new LocationDto(nextKeyPoint.Location.Latitude, nextKeyPoint.Location.Longitude))),
            options => options
                .Using<DateTime>(context => context.Subject.Should().BeCloseTo(context.Expectation, StoredTimePrecision))
                .WhenTypeIs<DateTime>());
        body.Should().NotContain(nextKeyPoint.Secret!);
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

    // The fourth tourist completed the first key point before the fifth one did, so a leak of their secrets shows as an earlier time.
    [Fact]
    public async Task Discovered_secrets_of_a_tour_include_the_key_points_completed_in_the_tourists_abandoned_and_completed_sessions_once_each_and_not_another_tourists()
    {
        var client = Factory.CreateClientFor(TourExecutionSeed.FifthExplorer, "explorer");
        var tour = TourSeed.PublishedVineyards;

        var secrets = await client.GetFromJsonAsync<List<DiscoveredSecretDto>>($"/api/exploration/tour-executions/discovered-secrets/{tour.Id}", JsonOptions);

        var firstKeyPoint = tour.KeyPoints[0];
        var secondKeyPoint = tour.KeyPoints[1];
        secrets.Should().BeEquivalentTo(
            [
                new DiscoveredSecretDto(firstKeyPoint.Id, firstKeyPoint.Name, firstKeyPoint.Secret, TourExecutionSeed.FifthExplorersAbandonedExecutionPastTheFirstKeyPoint.KeyPointCompletions[0].CompletedAt),
                new DiscoveredSecretDto(secondKeyPoint.Id, secondKeyPoint.Name, secondKeyPoint.Secret, TourExecutionSeed.FifthExplorersCompletedExecution.KeyPointCompletions[1].CompletedAt)
            ],
            options => options
                .WithStrictOrdering()
                .Using<DateTime>(context => context.Subject.Should().BeCloseTo(context.Expectation, StoredTimePrecision))
                .WhenTypeIs<DateTime>());
    }
}
