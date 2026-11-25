using Games.Application.SocialChallengeSolving;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeRunReadRepository
{
    Task<List<Guid>> GetSolvedChallengeIdsAsync(Guid userId);

    // The run in which the user solved the challenge, otherwise the active run they joined; null when there is neither.
    // A participant counts as active when they were in range at a check made at activeSince or later.
    Task<ParticipationDto?> GetParticipationAsync(Guid challengeId, Guid userId, DateTime activeSince);
}
