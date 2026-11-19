namespace Games.Application.LocationChallengePlaying;

public sealed record PlayedLocationChallengeDto(Guid Id, List<PlayedHiddenPointDto> HiddenPoints);
