using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeRepository
{
    void Add(LocationChallenge challenge);
}
