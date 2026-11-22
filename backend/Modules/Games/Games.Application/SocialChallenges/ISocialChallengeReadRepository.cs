namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
    Task<List<SocialChallengeDto>> GetAllAsync();
}
