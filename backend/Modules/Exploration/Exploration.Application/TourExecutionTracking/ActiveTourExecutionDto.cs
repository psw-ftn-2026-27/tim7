using Exploration.Application.Tours;

namespace Exploration.Application.TourExecutionTracking;

public sealed record ActiveTourExecutionDto(
    Guid Id,
    Guid TourId,
    string TourName,
    LocationDto LastKnownLocation,
    List<CompletedKeyPointDto> CompletedKeyPoints,
    TourExecutionKeyPointDto? NextKeyPoint);
