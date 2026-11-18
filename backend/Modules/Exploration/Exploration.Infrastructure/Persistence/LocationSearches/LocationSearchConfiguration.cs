using Exploration.Domain.LocationSearches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.LocationSearches;

internal static class LocationSearchConfiguration
{
    public static void ConfigureLocationSearches(this ModelBuilder modelBuilder)
    {
        ConfigureLocationSearch(modelBuilder.Entity<LocationSearch>());
    }

    private static void ConfigureLocationSearch(EntityTypeBuilder<LocationSearch> builder)
    {
        builder.OwnsOne(locationSearch => locationSearch.Center);
        builder.Navigation(locationSearch => locationSearch.Center).IsRequired();

        builder.OwnsOne(locationSearch => locationSearch.Radius);
        builder.Navigation(locationSearch => locationSearch.Radius).IsRequired();

        builder.PrimitiveCollection(locationSearch => locationSearch.Difficulties)
            .ElementType(difficulty => difficulty.HasConversion<string>());
        builder.Property(locationSearch => locationSearch.Transport).HasConversion<string>().HasMaxLength(20);
    }
}
