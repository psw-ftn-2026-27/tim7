using Games.Application.SocialChallenges;
using Games.Application.SocialChallengeSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeRunReadRepository : ISocialChallengeRunReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRunReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Guid>> GetSolvedChallengeIdsAsync(Guid userId)
    {
        return _dbContext.SocialChallengeRuns
            .Where(run => run.Participants.Any(participant => participant.UserId == userId && participant.SolvedAt != null))
            .Select(run => run.ChallengeId)
            .ToListAsync();
    }

    public Task<ParticipationDto?> GetParticipationAsync(Guid challengeId, Guid userId, DateTime activeSince)
    {
        return _dbContext.SocialChallengeRuns
            .Where(run => run.ChallengeId == challengeId)
            .Where(run => run.Participants.Any(participant =>
                participant.UserId == userId && (participant.SolvedAt != null || run.CompletedAt == null)))
            .Select(run => new ParticipationDto(
                run.Participants.Count(participant => participant.IsInRange && participant.LastCheckedAt >= activeSince),
                run.Participants
                    .Where(participant => participant.UserId == userId)
                    .Select(participant => participant.EarnedXp)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync();
    }
}
