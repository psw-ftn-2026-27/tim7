using Microsoft.EntityFrameworkCore;
using Social.Application.Conversations;
using Social.Application.DirectMessaging;

namespace Social.Infrastructure.Persistence.Conversations;

internal sealed class ConversationReadRepository : IConversationReadRepository
{
    private const string ChatKind = "conversation";

    private readonly SocialDbContext _dbContext;

    public ConversationReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The most recent conversation comes first; one without messages yet comes last.
    public async Task<List<ConversationDto>> GetForUserAsync(Guid userId)
    {
        var chats = _dbContext.Chats.Where(chat => chat.Context.Kind == ChatKind);

        var conversations = await _dbContext.Conversations
            .Where(conversation => conversation.UserAId == userId || conversation.UserBId == userId)
            .Select(conversation => new ConversationDto(
                conversation.Id,
                conversation.UserAId == userId ? conversation.UserBId : conversation.UserAId,
                null,
                _dbContext.Messages
                    .Where(message => chats.Any(chat => chat.Id == message.ChatId && chat.Context.ContextId == conversation.Id))
                    .Max(message => (DateTime?)message.SentAt)))
            .ToListAsync();

        return conversations.OrderByDescending(conversation => conversation.LastMessageAt ?? DateTime.MinValue).ToList();
    }
}
