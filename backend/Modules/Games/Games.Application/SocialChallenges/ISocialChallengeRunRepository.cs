using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeRunRepository
{
    // Null when the challenge has no run that is still waiting for enough participants.
    Task<SocialChallengeRun?> GetActiveAsync(Guid challengeId);

    // Null when the user has not joined the active run of the challenge.
    Task<SocialChallengeRun?> GetActiveWithParticipantAsync(Guid challengeId, Guid userId);

    // Null when the user has not solved the challenge.
    Task<SocialChallengeRun?> GetSolvedByAsync(Guid challengeId, Guid userId);

    void Add(SocialChallengeRun run);
}
