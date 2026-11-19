using Games.Application.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengePlaying;

public sealed class LocationChallengePlayingQueries
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly ILocationChallengeReadRepository _challengeReadRepository;
    private readonly ILocationChallengeProgressReadRepository _progressReadRepository;

    public LocationChallengePlayingQueries(
        ILocationChallengeRepository challengeRepository,
        ILocationChallengeReadRepository challengeReadRepository,
        ILocationChallengeProgressReadRepository progressReadRepository)
    {
        _challengeRepository = challengeRepository;
        _challengeReadRepository = challengeReadRepository;
        _progressReadRepository = progressReadRepository;
    }

    // The radius is derived by the aggregate, so the challenges are loaded through the write repository.
    public async Task<List<PlayableLocationChallengeDto>> GetLocationChallengesAsync(Guid userId)
    {
        var challenges = await _challengeRepository.GetAllAsync();
        var progresses = await _progressReadRepository.GetAllOfUserAsync(userId);

        return challenges
            .Select(challenge => new PlayableLocationChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                challenge.RadiusMeters(),
                StateOf(progresses.SingleOrDefault(progress => progress.LocationChallengeId == challenge.Id))))
            .ToList();
    }

    public async Task<PlayedLocationChallengeDto> GetLocationChallengeAsync(Guid userId, Guid locationChallengeId)
    {
        var photos = await _challengeReadRepository.GetHiddenPointPhotosAsync(locationChallengeId);
        if (photos is null)
        {
            throw new NotFoundException($"Location challenge {locationChallengeId} does not exist.");
        }

        var discoveredIds = await _progressReadRepository.GetDiscoveredPointIdsAsync(userId, locationChallengeId);
        if (discoveredIds is null)
        {
            throw new NotFoundException($"The challenge {locationChallengeId} is not started.");
        }

        var points = photos
            .Select(photo => new PlayedHiddenPointDto(photo.Id, photo.Image, discoveredIds.Contains(photo.Id)))
            .ToList();

        return new PlayedLocationChallengeDto(locationChallengeId, points);
    }

    private static LocationChallengeState StateOf(LocationChallengeProgressDto? progress)
    {
        if (progress is null)
        {
            return LocationChallengeState.NotStarted;
        }
        return progress.IsCompleted ? LocationChallengeState.Completed : LocationChallengeState.Active;
    }
}
