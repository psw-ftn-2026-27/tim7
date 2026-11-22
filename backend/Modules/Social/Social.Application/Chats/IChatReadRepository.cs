using Shared.Domain.Chats;
using Social.Application.ClubChat;

namespace Social.Application.Chats;

public interface IChatReadRepository
{
    Task<ChatStateDto?> GetByContextAsync(ChatContext context);
}
