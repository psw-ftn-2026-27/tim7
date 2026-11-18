using Exploration.Application;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.LocationSearches;
using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.TourExecutions;
using Exploration.Application.TourExecutionTracking;
using Exploration.Application.TourPreferences;
using Exploration.Application.TourPreferenceSetup;
using Exploration.Application.Tours;
using Exploration.Contracts;
using Exploration.Domain.TourExecutions;
using Exploration.Infrastructure.Images;
using Exploration.Infrastructure.Persistence;
using Exploration.Infrastructure.Persistence.LocationSearches;
using Exploration.Infrastructure.Persistence.TourExecutions;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Images;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExplorationDbContext>(configuration, "exploration");
        services.AddHostedService<ExplorationModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<ExplorationMapperProfile>());

        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ExplorationDbContext>());
        services.AddScoped<ITourReadRepository, TourReadRepository>();
        services.AddScoped<ITourPreferenceRepository, TourPreferenceRepository>();
        services.AddScoped<ITourPreferenceReadRepository, TourPreferenceReadRepository>();
        services.AddScoped<ILocationSearchRepository, LocationSearchRepository>();
        services.AddScoped<ITourExecutionRepository, TourExecutionRepository>();
        services.AddScoped<ITourExecutionReadRepository, TourExecutionReadRepository>();
        services.AddScoped<IImageStore, ExplorationImageStore>();
        services.AddScoped<IImageUsage, ExplorationImageUsage>();

        services.AddScoped<TourAuthoringService>();
        services.AddScoped<TourAuthoringQueries>();
        services.AddScoped<KeyPointAuthoringService>();
        services.AddScoped<KeyPointAuthoringQueries>();
        services.AddScoped<TourBrowsingService>();
        services.AddScoped<TourBrowsingQueries>();
        services.AddScoped<TourPreferenceSetupService>();
        services.AddScoped<TourPreferenceSetupQueries>();
        services.AddScoped<TourExecutionStarter>();
        services.AddScoped<TourExecutionTrackingService>();
        services.AddScoped<TourExecutionTrackingQueries>();
        services.AddScoped<IExplorationApi, ExplorationApi>();

        return services;
    }
}
