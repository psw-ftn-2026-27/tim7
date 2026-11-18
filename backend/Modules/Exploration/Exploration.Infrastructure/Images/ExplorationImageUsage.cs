using Exploration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Images;

namespace Exploration.Infrastructure.Images;

// Tells the platform's image cleanup job which images the module still uses: every key point image, whatever the tour's status.
internal sealed class ExplorationImageUsage : IImageUsage
{
    private readonly ExplorationDbContext _dbContext;

    public ExplorationImageUsage(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string Module => ExplorationImageStore.Module;

    public async Task<IReadOnlySet<string>> GetUsedAddressesAsync()
    {
        var addresses = await _dbContext.Tours
            .SelectMany(tour => tour.KeyPoints)
            .Select(keyPoint => keyPoint.Image)
            .ToListAsync();

        return addresses.ToHashSet();
    }
}
