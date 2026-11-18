using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Tokens;

namespace Payment.Infrastructure.Persistence.Tokens;

internal static class PurchaseTokenConfiguration
{
    public static void ConfigurePurchaseTokens(this ModelBuilder modelBuilder)
    {
        ConfigurePurchaseToken(modelBuilder.Entity<PurchaseToken>());
    }

    private static void ConfigurePurchaseToken(EntityTypeBuilder<PurchaseToken> builder)
    {
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.HasIndex(token => new { token.UserId, token.EntityId }).IsUnique();
    }
}
