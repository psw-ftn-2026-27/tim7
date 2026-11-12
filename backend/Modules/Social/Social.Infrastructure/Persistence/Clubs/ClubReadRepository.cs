using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClubDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.Id == id)).FirstOrDefaultAsync();
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.OwnerId == ownerId)).ToListAsync();
    }

    public async Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize)
    {
        var all = _dbContext.Clubs
            .OrderBy(club => club.Name)
            .ThenBy(club => club.Id);

        var totalCount = await all.CountAsync();
        var items = await ProjectToDtos(all)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<ClubDto>(items, totalCount);
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs
            .Select(club => new ClubDto(
                club.Id,
                club.OwnerId,
                club.Name,
                club.Description,
                club.Images));
    }
}
