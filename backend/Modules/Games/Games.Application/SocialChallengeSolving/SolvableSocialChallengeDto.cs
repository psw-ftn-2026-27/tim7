namespace Games.Application.SocialChallengeSolving;

public sealed record SolvableSocialChallengeDto(Guid Id, string Name, TimeOnly Start, TimeOnly End, int People, bool IsSolved);
