namespace Games.Application.SocialChallengeAuthoring;

public sealed record CreateSocialChallengeDto(
    string Name,
    TimeOnly Start,
    TimeOnly End,
    int People,
    int RangeMeters,
    int Xp,
    string ExitMessage);
