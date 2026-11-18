using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Social.Application.Chats;
using Social.Application.ClubChat;

namespace Social.Infrastructure.Persistence.Chats;

internal sealed class ChatReadRepository : IChatReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ChatReadRepository(SocialDbContext dbContext)
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
