using Shared.Domain.Chats;

namespace Exploration.Application.Chats;

public interface IChatRepository
{
    Task<Chat?> GetByContextAsync(ChatContext context);
    void Add(Chat chat);
}
