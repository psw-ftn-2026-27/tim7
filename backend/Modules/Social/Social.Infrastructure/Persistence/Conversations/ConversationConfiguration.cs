using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal static class ConversationConfiguration
{
    public static void ConfigureConversations(this ModelBuilder modelBuilder)
    {
        ConfigureConversation(modelBuilder.Entity<Conversation>());
    }

    private static void ConfigureConversation(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        // One pair of users has at most one conversation (P3).
        builder.HasIndex(conversation => new { conversation.UserAId, conversation.UserBId }).IsUnique();
    }
}
