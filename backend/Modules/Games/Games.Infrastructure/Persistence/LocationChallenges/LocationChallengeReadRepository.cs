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

    public async Task<List<HiddenPointPhotoDto>?> GetHiddenPointPhotosAsync(Guid locationChallengeId)
    {
        var points = await _dbContext.LocationChallenges
            .Where(challenge => challenge.Id == locationChallengeId)
            .Select(challenge => challenge.HiddenPoints
                .Select(point => new HiddenPointPhotoDto(point.Id, point.Image))
                .ToList())
            .ToListAsync();

        return points.SingleOrDefault();
    }
}
