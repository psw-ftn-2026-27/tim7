using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferenceSetup;

public sealed record SaveTourPreferenceDto(
    List<TourDifficulty> Difficulties,
    TransportMode? PreferredTransportMode,
    List<string> Tags);
