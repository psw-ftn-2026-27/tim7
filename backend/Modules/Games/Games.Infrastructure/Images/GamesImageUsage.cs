using Games.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Images;

namespace Games.Infrastructure.Images;

// Tells the platform's cleanup job which images the Games module still uses.
internal sealed class GamesImageUsage : IImageUsage
{
    private readonly GamesDbContext _dbContext;

    public GamesImageUsage(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string Module => GamesImageStore.Module;

    public async Task<IReadOnlySet<string>> GetUsedAddressesAsync()
    {
        var addresses = await _dbContext.LocationChallenges
            .SelectMany(challenge => challenge.HiddenPoints)
            .Select(point => point.Image)
            .ToListAsync();

        return addresses.ToHashSet();
    }
}
