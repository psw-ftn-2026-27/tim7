using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.Tours;

internal static class TourConfiguration
{
    public static void ConfigureTours(this ModelBuilder modelBuilder)
    {
        ConfigureTour(modelBuilder.Entity<Tour>());
    }

    private static void ConfigureTour(EntityTypeBuilder<Tour> builder)
    {
        builder.Property(tour => tour.Name).HasMaxLength(200);
        builder.Property(tour => tour.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(tour => tour.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(tour => tour.RecommendedGroupSize);
        builder.Navigation(tour => tour.RecommendedGroupSize).IsRequired();

        builder.OwnsOne(tour => tour.Length);

        // A draft has no price, so the price columns allow null.
        builder.OwnsOne(tour => tour.Price);

        builder.OwnsMany(tour => tour.TransportTimes, transportTimes =>
        {
            transportTimes.ToJson();
            transportTimes.Property(time => time.Transport).HasConversion<string>();
        });

        builder.OwnsMany(tour => tour.KeyPoints, keyPoints =>
        {
            keyPoints.ToTable("KeyPoints");
            keyPoints.WithOwner().HasForeignKey("TourId");
            keyPoints.HasKey(keyPoint => keyPoint.Id);
            // The ID is generated in the constructor; without this EF would take a new key point for an existing row.
            keyPoints.Property(keyPoint => keyPoint.Id).ValueGeneratedNever();
            keyPoints.Property(keyPoint => keyPoint.Name).HasMaxLength(200);
            keyPoints.OwnsOne(keyPoint => keyPoint.Location);
            keyPoints.Navigation(keyPoint => keyPoint.Location).IsRequired();
        });
    }
}
