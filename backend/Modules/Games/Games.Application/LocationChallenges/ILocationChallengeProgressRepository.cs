using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeProgressRepository
{
    Task<LocationChallengeProgress?> GetAsync(Guid userId, Guid locationChallengeId);
    void Add(LocationChallengeProgress progress);
}
