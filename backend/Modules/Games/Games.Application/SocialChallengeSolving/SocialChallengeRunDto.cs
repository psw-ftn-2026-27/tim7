namespace Games.Application.SocialChallengeSolving;

// Exactly one of Progress and Result is set: the progress while the challenge is unsolved, the result once it is solved.
public sealed record SocialChallengeRunDto(GatheringProgressDto? Progress, SocialChallengeResultDto? Result);

public sealed record GatheringProgressDto(int ActiveParticipantCount, int RequiredPeople);

public sealed record SocialChallengeResultDto(string ExitMessage, int EarnedXp);
