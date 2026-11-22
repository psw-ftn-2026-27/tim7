using Exploration.Application;
using Exploration.Domain.LocationSearches;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence.LocationSearches;
using Exploration.Infrastructure.Persistence.TourExecutions;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Chats;
using Shared.Infrastructure.Chats;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options) { }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourPreference> TourPreferences => Set<TourPreference>();
    public DbSet<LocationSearch> LocationSearches => Set<LocationSearch>();
    public DbSet<TourExecution> TourExecutions => Set<TourExecution>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ConfigureTours();
        builder.ConfigureTourPreferences();
        builder.ConfigureLocationSearches();
        builder.ConfigureTourExecutions();
        builder.ConfigureChats();
        builder.ConfigureMessages();
    }
}
