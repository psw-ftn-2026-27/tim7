using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeRepository
{
    Task<LocationChallenge?> GetByIdAsync(Guid id);
    Task<List<LocationChallenge>> GetAllAsync();
    void Add(LocationChallenge challenge);
}
