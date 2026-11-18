using Shared.Domain.Chats;

namespace Social.Application.Chats;

public interface IMessageRepository
{
    Task<Message?> GetInChatAsync(Guid chatId, Guid messageId);
    void Add(Message message);
}
