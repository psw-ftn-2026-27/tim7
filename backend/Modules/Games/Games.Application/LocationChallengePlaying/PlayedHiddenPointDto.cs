namespace Games.Application.LocationChallengePlaying;

// Carries the photo and whether the point is discovered; the location of the point never leaves the server.
public sealed record PlayedHiddenPointDto(Guid Id, string Image, bool IsDiscovered);
