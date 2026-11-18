using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Images;

public static class ImageCleanupExtensions
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan DefaultMinAge = TimeSpan.FromDays(1);

    // Task.Delay cannot wait longer than about 49 days, so a longer interval is rejected at startup.
    private static readonly TimeSpan MaxInterval = TimeSpan.FromDays(49);

    public static IServiceCollection AddImageCleanup(this IServiceCollection services, IConfiguration configuration)
    {
        var interval = ReadPositiveTimeSpan(configuration, "ImageCleanup:Interval", DefaultInterval);
        var minAge = ReadPositiveTimeSpan(configuration, "ImageCleanup:MinAge", DefaultMinAge);
        if (interval > MaxInterval)
        {
            throw new InvalidOperationException("Configuration value 'ImageCleanup:Interval' must not be longer than 49 days.");
        }

        return services.AddHostedService(provider => new ImageCleanupJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ImageStore>(),
            provider.GetRequiredService<ILogger<ImageCleanupJob>>(),
            interval,
            minAge));
    }

    // A missing value falls back to the default; a value that is present but invalid stops the server at startup.
    private static TimeSpan ReadPositiveTimeSpan(IConfiguration configuration, string key, TimeSpan defaultValue)
    {
        var value = configuration[key];
        if (value is null)
        {
            return defaultValue;
        }

        if (!TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpan) || timeSpan <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"Configuration value '{key}' must be a positive time span such as '1.00:00:00'.");
        }

        return timeSpan;
    }
}
