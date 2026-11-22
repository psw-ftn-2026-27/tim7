namespace Exploration.Application.TourExecutionTracking;

// A key point the tourist completed in the active session; only a completed key point carries its secret.
public sealed record CompletedKeyPointDto(
    Guid Id,
    int Order,
    string Name,
    DateTime CompletedAt,
    string? Secret);
