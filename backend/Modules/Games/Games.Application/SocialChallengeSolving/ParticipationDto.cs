namespace Games.Application.SocialChallengeSolving;

// EarnedXp is set once the user has solved the challenge.
public sealed record ParticipationDto(int ActiveParticipantCount, int? EarnedXp);
