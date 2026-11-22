using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Social.Application.Chats;

namespace Social.Infrastructure.Persistence.Chats;

internal sealed class MessageRepository : IMessageRepository
{
    private readonly SocialDbContext _dbContext;

    public MessageRepository(SocialDbContext dbContext)
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
