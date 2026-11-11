using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeLifecycle;

public sealed class SocialChallengeLifecycleService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeLifecycleService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.Restore();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid id)
    {
        var challenge = await _challengeRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {id} does not exist.");
        }
        return challenge;
    }
}
