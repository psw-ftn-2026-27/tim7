using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeRunRepository : ISocialChallengeRunRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRunRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallengeRun?> GetActiveAsync(Guid challengeId)
    {
        return _dbContext.SocialChallengeRuns.FirstOrDefaultAsync(run =>
            run.ChallengeId == challengeId && run.CompletedAt == null);
    }

    public Task<SocialChallengeRun?> GetActiveWithParticipantAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.SocialChallengeRuns.FirstOrDefaultAsync(run =>
            run.ChallengeId == challengeId
            && run.CompletedAt == null
            && run.Participants.Any(participant => participant.UserId == userId));
    }

    public Task<SocialChallengeRun?> GetSolvedByAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.SocialChallengeRuns.FirstOrDefaultAsync(run =>
            run.ChallengeId == challengeId
            && run.Participants.Any(participant => participant.UserId == userId && participant.SolvedAt != null));
    }

    public void Add(SocialChallengeRun run)
    {
        _dbContext.SocialChallengeRuns.Add(run);
    }
}
