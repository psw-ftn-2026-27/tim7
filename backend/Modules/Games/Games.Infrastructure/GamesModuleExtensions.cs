using Games.Application;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengePlaying;
using Games.Application.LocationChallenges;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengeLifecycle;
using Games.Application.SocialChallenges;
using Games.Infrastructure.Images;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Images;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();

        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<ILocationChallengeProgressRepository, LocationChallengeProgressRepository>();
        services.AddScoped<ILocationChallengeProgressReadRepository, LocationChallengeProgressReadRepository>();
        services.AddScoped<IImageStore, GamesImageStore>();
        services.AddScoped<IImageUsage, GamesImageUsage>();

        services.AddScoped<SocialChallengeAuthoringService>();
        services.AddScoped<SocialChallengeAuthoringQueries>();
        services.AddScoped<SocialChallengeLifecycleService>();
        services.AddScoped<LocationChallengeAuthoringService>();
        services.AddScoped<LocationChallengeAuthoringQueries>();
        services.AddScoped<LocationChallengePlayingService>();
        services.AddScoped<LocationChallengePlayingQueries>();

        return services;
    }
}
