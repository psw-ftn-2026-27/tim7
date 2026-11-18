using Identity.Core;
using Identity.Core.Connections;
using Identity.Core.UserPositions;
using Identity.Infrastructure.Connections;
using Identity.Infrastructure.UserPositions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityModuleDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
{
    public IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options) : base(options) { }

    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<UserPosition> UserPositions => Set<UserPosition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");
        builder.ConfigureConnections();
        builder.ConfigureUserPositions();
    }
}
