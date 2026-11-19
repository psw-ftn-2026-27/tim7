using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Carts;
using Payment.Application.Tokens;
using Payment.Application.TourPurchasing;
using Payment.Contracts;
using Payment.Domain.Checkout;
using Payment.Infrastructure.Persistence.Carts;
using Payment.Infrastructure.Persistence.Tokens;
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
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IPurchaseTokenRepository, PurchaseTokenRepository>();
        services.AddScoped<IPurchaseTokenReadRepository, PurchaseTokenReadRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());

        services.AddScoped<CartCheckout>();
        services.AddScoped<IPaymentApi, PaymentApi>();
        services.AddScoped<TourPurchasingService>();
        services.AddScoped<TourPurchasingQueries>();

        services.AddScoped<WalletOpeningService>();
        services.AddScoped<WalletBrowsingQueries>();
        services.AddScoped<WalletAdministrationService>();
        services.AddScoped<WalletAdministrationQueries>();

        return services;
    }
}
