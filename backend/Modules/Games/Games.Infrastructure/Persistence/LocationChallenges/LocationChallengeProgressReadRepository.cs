using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeProgressReadRepository : ILocationChallengeProgressReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeProgressReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<LocationChallengeProgressDto>> GetAllOfUserAsync(Guid userId)
    {
        return _dbContext.LocationChallengeProgresses
            .Where(progress => progress.UserId == userId)
            .Select(progress => new LocationChallengeProgressDto(progress.LocationChallengeId, progress.CompletedAt != null))
            .ToListAsync();
    }

    public async Task<List<Guid>?> GetDiscoveredPointIdsAsync(Guid userId, Guid locationChallengeId)
    {
        var discovered = await _dbContext.LocationChallengeProgresses
            .Where(progress => progress.UserId == userId && progress.LocationChallengeId == locationChallengeId)
            .Select(progress => progress.DiscoveredPointIds.ToList())
            .ToListAsync();

        return discovered.SingleOrDefault();
    }
}
