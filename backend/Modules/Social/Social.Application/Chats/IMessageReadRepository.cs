using Shared.Domain;

namespace Social.Application.Chats;

public interface IMessageReadRepository
{
    Task<PageResult<ChatMessageDto>> GetNewestAsync(Guid chatId, int page, int pageSize);
}
