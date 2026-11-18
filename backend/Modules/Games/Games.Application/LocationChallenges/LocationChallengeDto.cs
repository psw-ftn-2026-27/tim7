namespace Games.Application.LocationChallenges;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    LocationDto StartLocation,
    int Xp,
    int HiddenPointCount);
