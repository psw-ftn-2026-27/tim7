using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Exploration.Application.Chats;
using Exploration.Application.TourChat;

namespace Exploration.Infrastructure.Persistence.Chats;

internal sealed class MessageReadRepository : IMessageReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public MessageReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Deleted messages are already hidden by the shared mapping's query filter.
    public async Task<PageResult<ChatMessageDto>> GetNewestAsync(Guid chatId, int page, int pageSize)
    {
        var messages = _dbContext.Messages
            .Where(message => message.ChatId == chatId)
            .OrderByDescending(message => message.SentAt);

        var totalCount = await messages.CountAsync();
        var items = await messages
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(message => new ChatMessageDto(
                message.Id, message.AuthorId, null, message.Text, message.SentAt, false))
            .ToListAsync();

        return new PageResult<ChatMessageDto>(items, totalCount);
    }
}
