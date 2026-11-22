using Microsoft.EntityFrameworkCore;
using Social.Application.Conversations;
using Social.Domain.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal sealed class ConversationRepository : IConversationRepository
{
    private readonly SocialDbContext _dbContext;

    public ConversationRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Conversation?> GetByIdAsync(Guid id)
    {
        return _dbContext.Conversations.FirstOrDefaultAsync(conversation => conversation.Id == id);
    }

    public Task<Conversation?> GetBetweenAsync(Guid userId, Guid otherUserId)
    {
        return _dbContext.Conversations.FirstOrDefaultAsync(conversation =>
            (conversation.UserAId == userId && conversation.UserBId == otherUserId)
            || (conversation.UserAId == otherUserId && conversation.UserBId == userId));
    }

    public void Add(Conversation conversation)
    {
        _dbContext.Conversations.Add(conversation);
    }
}
