using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Chats;

namespace Shared.Infrastructure.Chats;

public static class ChatConfiguration
{
    public static void ConfigureChats(this ModelBuilder modelBuilder)
    {
        ConfigureChat(modelBuilder.Entity<Chat>());
    }

    private static void ConfigureChat(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chats");

        // One place of embedding (one club, one tour) has at most one chat.
        builder.OwnsOne(chat => chat.Context, context =>
        {
            context.Property(c => c.Kind).HasMaxLength(50);
            context.HasIndex(c => new { c.Kind, c.ContextId }).IsUnique();
        });
        builder.Navigation(chat => chat.Context).IsRequired();
    }
}
