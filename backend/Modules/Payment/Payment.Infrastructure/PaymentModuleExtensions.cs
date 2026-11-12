using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.WalletAdministration;
using Payment.Application.WalletBrowsing;
using Payment.Application.WalletOpening;
using Payment.Application.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());

        services.AddScoped<WalletOpeningService>();
        services.AddScoped<WalletBrowsingQueries>();
        services.AddScoped<WalletAdministrationService>();
        services.AddScoped<WalletAdministrationQueries>();

        return services;
    }
}
