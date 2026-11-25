using Games.Application.LocationChallenges;
using Games.Application.SocialChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly ISocialChallengeRunRepository _runRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeSolvingService(
        ISocialChallengeRepository challengeRepository,
        ISocialChallengeRunRepository runRepository,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _runRepository = runRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid userId, Guid challengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        // The active window is a time of day as the administrator entered it, so it is compared with the server's local time.
        challenge.EnsureCanBeActivatedFrom(ToLocation(position), TimeOnly.FromDateTime(DateTime.Now));

        var hasSolvedBefore = await _runRepository.GetSolvedByAsync(challengeId, userId) is not null;
        var run = await _runRepository.GetActiveAsync(challengeId);
        if (run is null)
        {
            run = new SocialChallengeRun(challengeId, userId);
            _runRepository.Add(run);
        }
        // Called for a new run too, so that the run alone decides whether a user who solved the challenge may join.
        run.Join(userId, hasSolvedBefore);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReportPositionAsync(Guid userId, Guid challengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var run = await _runRepository.GetActiveWithParticipantAsync(challengeId, userId);
        if (run is null)
        {
            throw new NotFoundException($"You have not joined an active run of the challenge {challengeId}.");
        }

        run.ReportPosition(
            userId,
            challenge.IsInRange(ToLocation(position)),
            DateTime.UtcNow,
            challenge.Gathering.People,
            challenge.Xp);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }
        return challenge;
    }

    private static Location ToLocation(LocationDto position)
    {
        return new Location(position.Latitude, position.Longitude);
    }
}
