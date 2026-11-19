using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal static class SocialChallengeConfiguration
{
    public static void ConfigureSocialChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureSocialChallenge(modelBuilder.Entity<SocialChallenge>());
    }

    private static void ConfigureSocialChallenge(EntityTypeBuilder<SocialChallenge> builder)
    {
        builder.Property(challenge => challenge.Name).HasMaxLength(50);
        builder.Property(challenge => challenge.ExitMessage).HasMaxLength(200);
        builder.Property(challenge => challenge.Status).HasConversion<string>().HasMaxLength(20);

        builder.ComplexProperty(challenge => challenge.ActiveWindow);
        builder.ComplexProperty(challenge => challenge.Gathering);
    }
}
