using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// A key point as the public offer shows it. It has no secret field, so the secret never leaves the server.
public sealed record PublicKeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string Image,
    LocationDto Location);
