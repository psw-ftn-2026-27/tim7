using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeProgressRepository : ILocationChallengeProgressRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeProgressRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeProgress?> GetAsync(Guid userId, Guid locationChallengeId)
    {
        return _dbContext.LocationChallengeProgresses.FirstOrDefaultAsync(progress =>
            progress.UserId == userId && progress.LocationChallengeId == locationChallengeId);
    }

    public void Add(LocationChallengeProgress progress)
    {
        _dbContext.LocationChallengeProgresses.Add(progress);
    }
}
