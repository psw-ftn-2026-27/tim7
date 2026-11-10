using Identity.Core.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Connections;

internal static class ConnectionConfiguration
{
    public static void ConfigureConnections(this ModelBuilder modelBuilder)
    {
        ConfigureConnection(modelBuilder.Entity<Connection>());
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
    }

    private static void ConfigureConnection(EntityTypeBuilder<Connection> builder)
    {
        builder.HasIndex(connection => new { connection.UserAId, connection.UserBId }).IsUnique();

        builder.HasMany(connection => connection.Requests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(connection => connection.Requests).AutoInclude();
    }

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
    }
}
