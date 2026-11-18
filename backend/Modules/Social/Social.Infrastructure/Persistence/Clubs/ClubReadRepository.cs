using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.ClubMembership;
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

    // Projects what the club page shows. Whether the caller is a member and may see the
    // pending requests is decided by the Club aggregate in ClubMembershipQueries.
    public async Task<ClubMembershipDto?> GetMembershipByIdAsync(Guid id)
    {
        var club = await _dbContext.Clubs
            .Where(club => club.Id == id)
            .Select(club => new ClubMembershipDto(
                club.Id,
                club.OwnerId,
                club.Name,
                club.Description,
                club.Images,
                club.Members
                    .OrderBy(member => member.UserId)
                    .Select(member => new ClubMemberDto(member.UserId, member.Role))
                    .ToList(),
                MembershipState.None,
                club.JoinRequests.Select(request => request.UserId).ToList()))
            .FirstOrDefaultAsync();
        if (club is null)
        {
            return null;
        }

        return club with { Members = [new ClubMemberDto(club.OwnerId, ClubRole.Owner), .. club.Members] };
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
