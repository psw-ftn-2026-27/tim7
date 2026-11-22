using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record RecordLocationSearchDto(
    LocationDto Center,
    double RadiusKilometers,
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    TransportMode? Transport,
    int? MinPrice,
    int? MaxPrice,
    decimal? MinLength,
    decimal? MaxLength,
    int ResultCount);
