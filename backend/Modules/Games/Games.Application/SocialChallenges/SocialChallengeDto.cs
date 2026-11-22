using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallenges;

public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    TimeOnly Start,
    TimeOnly End,
    int People,
    int RangeMeters,
    int Xp,
    string ExitMessage,
    SocialChallengeStatus Status);
