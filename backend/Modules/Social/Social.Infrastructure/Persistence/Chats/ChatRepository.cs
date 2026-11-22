using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Social.Application.Chats;

namespace Social.Infrastructure.Persistence.Chats;

internal sealed class ChatRepository : IChatRepository
{
    private readonly SocialDbContext _dbContext;

    public ChatRepository(SocialDbContext dbContext)
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
