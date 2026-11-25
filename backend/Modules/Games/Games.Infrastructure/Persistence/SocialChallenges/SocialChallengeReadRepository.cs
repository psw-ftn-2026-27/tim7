using Games.Application.LocationChallenges;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallengeDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.SocialChallenges.Where(challenge => challenge.Id == id)).FirstOrDefaultAsync();
    }

    public Task<List<SocialChallengeDto>> GetAllAsync()
    {
        return ProjectToDtos(_dbContext.SocialChallenges).ToListAsync();
    }

    public Task<List<SocialChallengeDto>> GetPublishedAsync()
    {
        var published = _dbContext.SocialChallenges
            .Where(challenge => challenge.Status == SocialChallengeStatus.Published)
            .OrderBy(challenge => challenge.Name);
        return ProjectToDtos(published).ToListAsync();
    }

    private static IQueryable<SocialChallengeDto> ProjectToDtos(IQueryable<SocialChallenge> challenges)
    {
        return challenges
            .Select(challenge => new SocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.ActiveWindow.Start,
                challenge.ActiveWindow.End,
                challenge.Gathering.People,
                challenge.Gathering.RangeMeters,
                challenge.Xp,
                challenge.ExitMessage,
                challenge.Status,
                challenge.Location == null ? null : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)));
    }
}
