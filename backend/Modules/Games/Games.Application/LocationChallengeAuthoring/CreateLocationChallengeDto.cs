using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record CreateLocationChallengeDto(
    string Name,
    int Xp,
    LocationDto? StartLocation,
    List<HiddenPointDto>? HiddenPoints);
