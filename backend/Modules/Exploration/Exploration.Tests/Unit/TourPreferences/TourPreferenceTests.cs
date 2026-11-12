using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class TourPreferenceTests
{
    private static TourPreference CreatePreferences() =>
        new(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Walking, ["istorija"]);

    [Fact]
    public void Preferences_without_any_criterion_cannot_be_set_up()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preferences_cannot_be_changed_to_have_no_criterion()
    {
        var preferences = CreatePreferences();

        var update = () => preferences.Update([], null, []);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preferences_with_only_a_difficulty_are_valid()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Hard], null, []);

        creation.Should().NotThrow();
    }

    [Fact]
    public void Preferences_with_only_a_preferred_transport_mode_are_valid()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], TransportMode.Bicycle, []);

        creation.Should().NotThrow();
    }

    [Fact]
    public void Preferences_with_only_a_tag_are_valid()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, ["priroda"]);

        creation.Should().NotThrow();
    }

    [Fact]
    public void New_preferences_are_not_applied_automatically()
    {
        var preferences = new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Easy], null, []);

        preferences.ApplyPreferencesAutomatically.Should().BeFalse();
    }
}
