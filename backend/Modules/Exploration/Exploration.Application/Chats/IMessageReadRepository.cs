using Shared.Domain;
using Exploration.Application.TourChat;

namespace Exploration.Application.Chats;

public interface IMessageReadRepository
{
    Task<PageResult<ChatMessageDto>> GetNewestAsync(Guid chatId, int page, int pageSize);
}
