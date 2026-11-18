using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Shared.Infrastructure.Chats;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.Clubs;
using Social.Domain.Conversations;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.Conversations;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Conversation> Conversations => Set<Conversation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureClubs();
        builder.ConfigureChats();
        builder.ConfigureMessages();
        builder.ConfigureConversations();
    }
}
