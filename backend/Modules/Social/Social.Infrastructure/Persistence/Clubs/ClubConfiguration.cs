using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal static class ClubConfiguration
{
    public static void ConfigureClubs(this ModelBuilder modelBuilder)
    {
        ConfigureClub(modelBuilder.Entity<Club>());
        ConfigureClubMember(modelBuilder.Entity<ClubMember>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.Property(club => club.Name).HasMaxLength(200);
        builder.Property(club => club.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ClubStatus.Active);

        builder.HasMany(club => club.Members)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(club => club.Members).AutoInclude();

        builder.OwnsMany(club => club.JoinRequests, joinRequests =>
        {
            joinRequests.ToTable("ClubJoinRequests");
            joinRequests.WithOwner().HasForeignKey("ClubId");
        });

        builder.OwnsMany(club => club.Invitations, invitations =>
        {
            invitations.ToTable("ClubInvitations");
            invitations.WithOwner().HasForeignKey("ClubId");
        });
    }

    private static void ConfigureClubMember(EntityTypeBuilder<ClubMember> builder)
    {
        builder.ToTable("ClubMembers");
        builder.Property(member => member.Id).ValueGeneratedNever();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(20);
    }
}
