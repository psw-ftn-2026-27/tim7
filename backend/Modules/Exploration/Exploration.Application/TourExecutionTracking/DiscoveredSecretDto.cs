namespace Exploration.Application.TourExecutionTracking;

// A key point of a tour the tourist completed in any of their sessions, with the earliest time they reached it.
public sealed record DiscoveredSecretDto(
    Guid KeyPointId,
    string Name,
    string? Secret,
    DateTime CompletedAt);
