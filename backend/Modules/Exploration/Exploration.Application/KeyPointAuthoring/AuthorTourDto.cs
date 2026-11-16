using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.KeyPointAuthoring;

// The author's own view of a tour. It carries the key point secrets, so it never serves the public offer (TourDto).
public sealed record AuthorTourDto(
    Guid Id,
    Guid AuthorId,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    TourStatus Status,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    RecommendedGroupSizeDto RecommendedGroupSize,
    TourLengthDto? Length,
    List<TransportTimeDto> TransportTimes,
    List<KeyPointDto> KeyPoints);
