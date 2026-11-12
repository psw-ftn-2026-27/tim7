using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceSetup;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceSetup;

public class TourPreferenceSetupCommandTests : BaseIntegrationTest
{
    public TourPreferenceSetupCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_save_sets_up_the_users_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new SaveTourPreferenceDto([TourDifficulty.Hard], TransportMode.Bicycle, ["planina"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var saved = await response.Content.ReadFromJsonAsync<TourPreferenceDto>(JsonOptions);
        saved.Should().BeEquivalentTo(request);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        var stored = assertContext.TourPreferences.Single(preference => preference.UserId == WellKnownUsers.SecondExplorer);
        stored.Should().BeEquivalentTo(request);
    }

    [Fact]
    public async Task Saving_again_changes_the_existing_preferences_without_creating_a_second_set()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SaveTourPreferenceDto([], null, ["vino"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.UserId == WellKnownUsers.Explorer);
        stored.Should().BeEquivalentTo(request);
    }

    [Fact]
    public async Task Preferences_without_any_criterion_are_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new SaveTourPreferenceDto([], null, [], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        assertContext.TourPreferences.Should().NotContain(preference => preference.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_preferences()
    {
        var client = Factory.CreateClient();
        var request = new SaveTourPreferenceDto([TourDifficulty.Easy], null, [], true);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveTourPreferenceDto([TourDifficulty.Easy], null, [], true);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deleting_removes_the_users_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore - 1);
        assertContext.TourPreferences.Should().NotContain(preference => preference.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Deleting_preferences_that_do_not_exist_reports_that_they_do_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_preferences()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
