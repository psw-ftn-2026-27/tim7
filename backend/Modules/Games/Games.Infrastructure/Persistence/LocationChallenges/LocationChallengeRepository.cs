using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeRepository : ILocationChallengeRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(LocationChallenge challenge)
    {
        _dbContext.LocationChallenges.Add(challenge);
    }
}
