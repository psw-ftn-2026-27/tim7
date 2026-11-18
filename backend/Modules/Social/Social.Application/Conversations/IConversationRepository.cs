using Social.Domain.Conversations;

namespace Social.Application.Conversations;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<Conversation?> GetBetweenAsync(Guid userId, Guid otherUserId);
    void Add(Conversation conversation);
}
