using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Exploration.Application.Chats;
using Exploration.Application.TourChat;

namespace Exploration.Infrastructure.Persistence.Chats;

internal sealed class ChatReadRepository : IChatReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public ChatReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ChatStateDto?> GetByContextAsync(ChatContext context)
    {
        return _dbContext.Chats
            .Where(chat => chat.Context.Kind == context.Kind && chat.Context.ContextId == context.ContextId)
            .Select(chat => new ChatStateDto(
                chat.Id,
                chat.IsLocked,
                chat.MutedUsers.Select(muted => muted.UserId).ToList()))
            .FirstOrDefaultAsync();
    }
}
