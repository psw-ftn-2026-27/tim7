using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeAuthoring;

// The photo of the point at position i travels as the image file at position i of the same request.
public sealed record HiddenPointDto(LocationDto? Location);
