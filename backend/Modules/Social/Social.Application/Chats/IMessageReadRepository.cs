using Shared.Domain;
using Social.Application.ClubChat;

namespace Social.Application.Chats;

public interface IMessageReadRepository
{
    Task<PageResult<ChatMessageDto>> GetNewestAsync(Guid chatId, int page, int pageSize);
}
