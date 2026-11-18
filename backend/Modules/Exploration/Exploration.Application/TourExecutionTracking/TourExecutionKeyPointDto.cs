using Exploration.Application.Tours;

namespace Exploration.Application.TourExecutionTracking;

// A key point as the active tour shows it. It has no secret field; secrets belong to progressing through the tour.
public sealed record TourExecutionKeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string Image,
    LocationDto Location);
