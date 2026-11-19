using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<LocationChallengeDto>> GetAllAsync()
    {
        return _dbContext.LocationChallenges
            .Select(challenge => new LocationChallengeDto(
                challenge.Id,
                challenge.Name,
                new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                challenge.Xp,
                challenge.HiddenPoints.Count))
            .ToListAsync();
    }
}
