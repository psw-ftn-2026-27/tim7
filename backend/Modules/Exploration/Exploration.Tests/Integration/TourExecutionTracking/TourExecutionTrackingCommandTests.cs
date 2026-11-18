using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecutionTracking;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecutionTracking;

public class TourExecutionTrackingCommandTests : BaseIntegrationTest
{
    private static readonly LocationDto StartLocation = new(45.2600, 19.8500);

    public TourExecutionTrackingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Starting_a_tour_the_tourist_may_walk_records_an_active_session_with_the_start_location_and_start_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new StartTourDto(TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint.Id, StartLocation);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var executionId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore + 1);
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == executionId);
        stored.TouristId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.TourId.Should().Be(TourSeed.PublishedFreeTourWithHiddenFirstKeyPoint.Id);
        stored.Status.Should().Be(TourExecutionStatus.Active);
        stored.StartLocation.Should().Be(new Location(StartLocation.Latitude, StartLocation.Longitude));
        stored.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Starting_the_tour_that_is_already_active_leads_to_the_existing_session_without_recording_a_new_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new StartTourDto(TourExecutionSeed.ExplorersActiveExecution.TourId, StartLocation);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var executionId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        executionId.Should().Be(TourExecutionSeed.ExplorersActiveExecution.Id);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Tourist_whose_session_of_the_tour_was_abandoned_gets_a_new_active_session_by_starting_it_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new StartTourDto(TourExecutionSeed.SecondExplorersAbandonedExecution.TourId, StartLocation);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var executionId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        executionId.Should().NotBe(TourExecutionSeed.SecondExplorersAbandonedExecution.Id);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore + 1);
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == executionId);
        stored.TourId.Should().Be(TourExecutionSeed.SecondExplorersAbandonedExecution.TourId);
        stored.Status.Should().Be(TourExecutionStatus.Active);
    }

    [Fact]
    public async Task Starting_a_tour_while_another_one_is_active_is_rejected_and_records_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new StartTourDto(TourSeed.PublishedOldTownWalk.Id, StartLocation);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
        assertContext.TourExecutions.Should().NotContain(execution =>
            execution.TouristId == WellKnownUsers.Explorer && execution.TourId == TourSeed.PublishedOldTownWalk.Id);
    }

    [Fact]
    public async Task Starting_a_tour_that_does_not_exist_reports_that_it_does_not_exist_and_records_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new StartTourDto(Guid.NewGuid(), StartLocation);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new StartTourDto(TourSeed.PublishedOldTownWalk.Id, StartLocation);

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new StartTourDto(TourSeed.PublishedOldTownWalk.Id, StartLocation);

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Abandoning_the_active_tour_records_the_abandoned_state_and_the_time_of_abandoning()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == TourExecutionSeed.ExplorersActiveExecution.Id);
        stored.Status.Should().Be(TourExecutionStatus.Abandoned);
        stored.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Abandoning_without_an_active_session_reports_that_it_does_not_exist_and_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
        assertContext.TourExecutions
            .Where(execution => execution.TouristId == WellKnownUsers.SecondExplorer)
            .Should().OnlyContain(execution => execution.Status == TourExecutionStatus.Abandoned);
    }

    [Fact]
    public async Task Anonymous_user_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
