using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceSetup;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceSetup;

public class TourPreferenceSetupQueryTests : BaseIntegrationTest
{
    public TourPreferenceSetupQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_gets_their_own_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var mine = await client.GetFromJsonAsync<MyTourPreferencesDto>("/api/exploration/tour-preferences/mine", JsonOptions);

        var seeded = TourPreferenceSeed.ExplorersPreferences;
        mine!.Preferences.Should().BeEquivalentTo(new TourPreferenceDto(seeded.Difficulties,
            seeded.PreferredTransportMode, seeded.Tags, seeded.ApplyPreferencesAutomatically));
    }

    [Fact]
    public async Task User_without_preferences_learns_that_they_are_not_set_up()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mine = await response.Content.ReadFromJsonAsync<MyTourPreferencesDto>(JsonOptions);
        mine!.Preferences.Should().BeNull();
    }

    [Fact]
    public async Task Offered_tags_come_only_from_published_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tags = await client.GetFromJsonAsync<List<string>>("/api/exploration/tour-preferences/available-tags", JsonOptions);

        var publishedTags = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.Tags)
            .Distinct();
        tags.Should().BeEquivalentTo(publishedTags);
    }
}
