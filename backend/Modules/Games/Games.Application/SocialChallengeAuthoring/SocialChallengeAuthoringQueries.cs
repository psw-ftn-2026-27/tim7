using Games.Application.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringQueries
{
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public SocialChallengeAuthoringQueries(ISocialChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<List<SocialChallengeDto>> GetAllAsync()
    {
        return _challengeReadRepository.GetAllAsync();
    }

    public async Task<SocialChallengeDto> GetByIdAsync(Guid id)
    {
        var challenge = await _challengeReadRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {id} does not exist.");
        }
        return challenge;
    }
}
