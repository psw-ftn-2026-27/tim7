using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourSearchDto(
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    TransportMode? Transport,
    SortDirection? DifficultySort);
