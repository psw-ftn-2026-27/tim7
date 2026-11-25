using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingQueries
{
    private readonly ISocialChallengeReadRepository _challengeReadRepository;
    private readonly ISocialChallengeRunReadRepository _runReadRepository;

    public SocialChallengeSolvingQueries(
        ISocialChallengeReadRepository challengeReadRepository,
        ISocialChallengeRunReadRepository runReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
        _runReadRepository = runReadRepository;
    }

    public async Task<List<SolvableSocialChallengeDto>> GetChallengesAsync(Guid userId)
    {
        var challenges = await _challengeReadRepository.GetPublishedAsync();
        var solvedChallengeIds = await _runReadRepository.GetSolvedChallengeIdsAsync(userId);

        return challenges
            .Select(challenge => new SolvableSocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Start,
                challenge.End,
                challenge.People,
                solvedChallengeIds.Contains(challenge.Id)))
            .ToList();
    }

    public async Task<SocialChallengeRunDto> GetRunAsync(Guid userId, Guid challengeId)
    {
        var challenge = await _challengeReadRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }

        var participation = await _runReadRepository.GetParticipationAsync(
            challengeId, userId, DateTime.UtcNow - SocialChallengeRun.MaxCheckAge);
        if (participation is null)
        {
            throw new NotFoundException($"You have not joined a run of the challenge {challengeId}.");
        }

        if (participation.EarnedXp is not null)
        {
            return new SocialChallengeRunDto(
                null,
                new SocialChallengeResultDto(challenge.ExitMessage, participation.EarnedXp.Value));
        }
        return new SocialChallengeRunDto(
            new GatheringProgressDto(participation.ActiveParticipantCount, challenge.People),
            null);
    }
}
