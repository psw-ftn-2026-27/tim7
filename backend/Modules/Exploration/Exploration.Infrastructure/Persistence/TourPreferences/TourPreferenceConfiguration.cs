using Exploration.Domain.TourPreferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal static class TourPreferenceConfiguration
{
    public static void ConfigureTourPreferences(this ModelBuilder modelBuilder)
    {
        ConfigureTourPreference(modelBuilder.Entity<TourPreference>());
    }

    private static void ConfigureTourPreference(EntityTypeBuilder<TourPreference> builder)
    {
        builder.HasIndex(tourPreference => tourPreference.UserId).IsUnique();
        builder.PrimitiveCollection(tourPreference => tourPreference.Difficulties)
            .ElementType(difficulty => difficulty.HasConversion<string>());
        builder.Property(tourPreference => tourPreference.PreferredTransportMode).HasConversion<string>().HasMaxLength(20);
    }
}
