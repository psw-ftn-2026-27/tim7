using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal static class SocialChallengeRunConfiguration
{
    public static void ConfigureSocialChallengeRuns(this ModelBuilder modelBuilder)
    {
        ConfigureSocialChallengeRun(modelBuilder.Entity<SocialChallengeRun>());
        ConfigureParticipant(modelBuilder.Entity<Participant>());
    }

    private static void ConfigureSocialChallengeRun(EntityTypeBuilder<SocialChallengeRun> builder)
    {
        builder.ToTable("SocialChallengeRuns");
        builder.Property(run => run.Id).ValueGeneratedNever();
        builder.HasMany(run => run.Participants).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(run => run.Participants).AutoInclude();
    }

    private static void ConfigureParticipant(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("SocialChallengeParticipants");
        // The ID is generated in the constructor; without this EF would take a new participant for an existing row.
        builder.Property(participant => participant.Id).ValueGeneratedNever();
    }
}
