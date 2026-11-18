using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeConfiguration
{
    public static void ConfigureLocationChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallenge(modelBuilder.Entity<LocationChallenge>());
        ConfigureHiddenPoint(modelBuilder.Entity<HiddenPoint>());
    }

    private static void ConfigureLocationChallenge(EntityTypeBuilder<LocationChallenge> builder)
    {
        builder.Property(challenge => challenge.Name).HasMaxLength(20);

        builder.ComplexProperty(challenge => challenge.StartLocation);

        builder.HasMany(challenge => challenge.HiddenPoints)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(challenge => challenge.HiddenPoints).AutoInclude();
    }

    private static void ConfigureHiddenPoint(EntityTypeBuilder<HiddenPoint> builder)
    {
        builder.ToTable("HiddenPoints");
        builder.Property(point => point.Id).ValueGeneratedNever();
        builder.Property(point => point.Image).HasMaxLength(200);

        builder.ComplexProperty(point => point.Location);
    }
}
