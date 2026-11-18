using Identity.Core.UserPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.UserPositions;

internal static class UserPositionConfiguration
{
    public static void ConfigureUserPositions(this ModelBuilder modelBuilder)
    {
        ConfigureUserPosition(modelBuilder.Entity<UserPosition>());
    }

    private static void ConfigureUserPosition(EntityTypeBuilder<UserPosition> builder)
    {
        builder.HasIndex(userPosition => userPosition.UserId).IsUnique();

        builder.OwnsOne(userPosition => userPosition.Coordinates);
        builder.Navigation(userPosition => userPosition.Coordinates).IsRequired();
    }
}
