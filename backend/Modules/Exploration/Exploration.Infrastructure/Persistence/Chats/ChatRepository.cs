using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Exploration.Application.Chats;

namespace Exploration.Infrastructure.Persistence.Chats;

internal sealed class ChatRepository : IChatRepository
{
    private readonly ExplorationDbContext _dbContext;

    public ChatRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Chat?> GetByContextAsync(ChatContext context)
    {
        return _dbContext.Chats.FirstOrDefaultAsync(chat =>
            chat.Context.Kind == context.Kind && chat.Context.ContextId == context.ContextId);
    }

    public void Add(Chat chat)
    {
        _dbContext.Chats.Add(chat);
    }
}
