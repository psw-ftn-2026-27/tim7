using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Exploration.Application.Chats;

namespace Exploration.Infrastructure.Persistence.Chats;

internal sealed class MessageRepository : IMessageRepository
{
    private readonly ExplorationDbContext _dbContext;

    public MessageRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Message?> GetInChatAsync(Guid chatId, Guid messageId)
    {
        return _dbContext.Messages.FirstOrDefaultAsync(message => message.ChatId == chatId && message.Id == messageId);
    }

    public void Add(Message message)
    {
        _dbContext.Messages.Add(message);
    }
}
