using Shared.Domain.Chats;

namespace Social.Application.Chats;

public interface IChatRepository
{
    Task<Chat?> GetByContextAsync(ChatContext context);
    void Add(Chat chat);
}
