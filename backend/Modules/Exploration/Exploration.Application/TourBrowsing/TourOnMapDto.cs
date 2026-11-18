using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// A tour on the map search page: its card data and where its marker goes. The location is
// that of the tour's first public key point, and it is null when the tour has no public key point.
public sealed record TourOnMapDto(
    TourDto Tour,
    LocationDto? FirstPublicKeyPointLocation);
