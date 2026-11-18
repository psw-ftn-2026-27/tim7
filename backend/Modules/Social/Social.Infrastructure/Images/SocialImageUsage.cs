using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Images;
using Social.Infrastructure.Persistence;

namespace Social.Infrastructure.Images;

// Tells the platform's cleanup job which images the Social module still uses, whatever the status of the aggregate.
internal sealed class SocialImageUsage : IImageUsage
{
    private readonly SocialDbContext _dbContext;

    public SocialImageUsage(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string Module => SocialImageStore.Module;

    public async Task<IReadOnlySet<string>> GetUsedAddressesAsync()
    {
        var blogImages = await _dbContext.Blogs.Select(blog => blog.Images).ToListAsync();
        var clubImages = await _dbContext.Clubs.Select(club => club.Images).ToListAsync();

        return blogImages.Concat(clubImages).SelectMany(images => images).ToHashSet();
    }
}
