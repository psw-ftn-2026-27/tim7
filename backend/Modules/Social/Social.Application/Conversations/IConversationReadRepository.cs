using Social.Application.DirectMessaging;

namespace Social.Application.Conversations;

public interface IConversationReadRepository
{
    Task<List<ConversationDto>> GetForUserAsync(Guid userId);
}
