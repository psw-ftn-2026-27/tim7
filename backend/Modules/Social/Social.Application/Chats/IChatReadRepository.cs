using Shared.Domain.Chats;

namespace Social.Application.Chats;

public interface IChatReadRepository
{
    Task<ChatStateDto?> GetByContextAsync(ChatContext context);
}
