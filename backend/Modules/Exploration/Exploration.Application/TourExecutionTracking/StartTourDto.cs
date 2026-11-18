using Exploration.Application.Tours;

namespace Exploration.Application.TourExecutionTracking;

public sealed record StartTourDto(Guid TourId, LocationDto StartLocation);
