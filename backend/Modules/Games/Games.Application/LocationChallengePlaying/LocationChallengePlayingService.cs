using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengePlaying;

public sealed class LocationChallengePlayingService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly ILocationChallengeProgressRepository _progressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengePlayingService(
        ILocationChallengeRepository challengeRepository,
        ILocationChallengeProgressRepository progressRepository,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _progressRepository = progressRepository;
        _unitOfWork = unitOfWork;
    }

    // The client activates on its own whenever the user is in the circle, so an already started challenge stays as it is.
    public async Task ActivateLocationChallengeAsync(Guid userId, Guid locationChallengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(locationChallengeId);
        challenge.EnsureInCircle(new Location(position.Latitude, position.Longitude));

        var progress = await _progressRepository.GetAsync(userId, locationChallengeId);
        if (progress is null)
        {
            _progressRepository.Add(new LocationChallengeProgress(userId, locationChallengeId));
        }
        else
        {
            progress.EnsureCanActivate();
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckHiddenPointAsync(Guid userId, Guid locationChallengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(locationChallengeId);
        var progress = await _progressRepository.GetAsync(userId, locationChallengeId);
        if (progress is null)
        {
            throw new NotFoundException($"The challenge {locationChallengeId} is not started.");
        }

        var point = challenge.GetUndiscoveredPointNear(
            new Location(position.Latitude, position.Longitude), progress.DiscoveredPointIds);
        progress.Discover(point.Id, challenge.HiddenPoints.Count, challenge.Xp);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid id)
    {
        var challenge = await _challengeRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {id} does not exist.");
        }
        return challenge;
    }
}
