using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Chats;

namespace Shared.Infrastructure.Chats;

public static class MessageConfiguration
{
    public static void ConfigureMessages(this ModelBuilder modelBuilder)
    {
        ConfigureMessage(modelBuilder.Entity<Message>());
    }

    private static void ConfigureMessage(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.Property(message => message.Text).HasMaxLength(Message.MaxTextLength);

        // A deleted message is never read again, by any module's repository (P9).
        builder.HasQueryFilter(message => !message.IsDeleted);
    }
}
