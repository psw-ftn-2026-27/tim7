using Identity.Core;
using Identity.Core.Connections;
using Identity.Core.Following;
using Identity.Infrastructure.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Identity.Infrastructure;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<IdentityModuleDbContext>(configuration, "identity");
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<JwtTokenFactory>();
        services.AddHostedService<IdentityModuleInitializer>();

        services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityModuleDbContext>();

        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IdentityModuleDbContext>());
        services.AddScoped<IConnectionReadRepository, ConnectionReadRepository>();

        services.AddScoped<FollowingService>();
        services.AddScoped<FollowingQueries>();

        return services;
    }
}
