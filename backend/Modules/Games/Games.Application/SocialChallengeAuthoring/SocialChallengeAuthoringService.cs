using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeAuthoringService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateSocialChallengeDto dto)
    {
        var challenge = new SocialChallenge(
            dto.Name, dto.Start, dto.End, dto.People, dto.RangeMeters, dto.Xp, dto.ExitMessage);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateSocialChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.Update(dto.Name, dto.Start, dto.End, dto.People, dto.RangeMeters, dto.Xp, dto.ExitMessage);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);

        challenge.EnsureDeletable();
        _challengeRepository.Remove(challenge);
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
