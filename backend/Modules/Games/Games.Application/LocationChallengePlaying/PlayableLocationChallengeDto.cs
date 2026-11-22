using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengePlaying;

public sealed record PlayableLocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    LocationDto StartLocation,
    double RadiusMeters,
    LocationChallengeState State);
