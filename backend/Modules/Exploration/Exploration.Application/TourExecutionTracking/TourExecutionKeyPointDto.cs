using Exploration.Application.Tours;

namespace Exploration.Application.TourExecutionTracking;

// The next key point as the active tour shows it. It has no secret field; only a completed key point reveals its secret.
public sealed record TourExecutionKeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string Image,
    LocationDto Location);
