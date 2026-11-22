namespace Games.Application.LocationChallenges;

public interface ILocationChallengeProgressReadRepository
{
    Task<List<LocationChallengeProgressDto>> GetAllOfUserAsync(Guid userId);

    // Null when the user has not started the challenge.
    Task<List<Guid>?> GetDiscoveredPointIdsAsync(Guid userId, Guid locationChallengeId);
}
