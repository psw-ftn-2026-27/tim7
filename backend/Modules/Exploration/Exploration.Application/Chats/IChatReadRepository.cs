using Shared.Domain.Chats;
using Exploration.Application.TourChat;

namespace Exploration.Application.Chats;

public interface IChatReadRepository
{
    Task<ChatStateDto?> GetByContextAsync(ChatContext context);
}
