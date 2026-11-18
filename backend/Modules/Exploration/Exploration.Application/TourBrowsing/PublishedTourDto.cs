using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

// The view of a published tour for someone who has not bought it. It lists only the public key points.
public sealed record PublishedTourDto(
    Guid Id,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    TourLengthDto? Length,
    List<TransportTimeDto> TransportTimes,
    List<PublicKeyPointDto> KeyPoints,
    bool IsFree);
